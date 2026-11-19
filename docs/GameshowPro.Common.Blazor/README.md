# GameshowPro.Common.Blazor

Shared Blazor components for GameshowPro service UIs — the web-tier analogue of the WPF
`RemoteService.xaml` templates in the `Client.Windows` builds. MudBlazor-styled (the standard
component library for the web tier); usable from Blazor WebAssembly and Blazor Server alike.

This package exists as a sibling of `GameshowPro.Common` (like `.JsonNet`, `.NLog`,
`.EngineIpc`) so that Razor and MudBlazor dependencies never leak into Common's other
consumers — WPF apps and NativeAOT services must not inherit them.

## ServiceStateUi

The standardized hierarchical `ServiceState` renderer (services architecture §3): recursive
tree of state icon, name, detail, and progress. Pass any node as `DataContext`; the component
subscribes to the node's `PropertyChanged`/`AllUpdated`/children events itself, so updates
render without any parent involvement.

```razor
<ServiceStateUi DataContext="_client.ServiceState" />
```

### Progress semantics (supported in full from day one)

`ServiceState.Progress` describes a finite process with an estimated completion:

| `Progress` | Meaning | Rendering |
|---|---|---|
| `null`, state ≠ Connected | busy for an unknown time | indeterminate bar (`gsp-progress-indeterminate`) |
| `null`, state = Connected | steady | nothing |
| `0 ≤ p < 1` | finite process, `p` complete | determinate bar + percentage (`gsp-progress-determinate`) |
| `p ≥ 1` | done / steady | nothing |

Producers publishing a long-running startup (caching, sync, device scan) can set fractional
progress and it renders correctly today. Steady nodes should publish `1` (or `null` only while
Connected); a `Disconnected`/`Warning` node with `null` progress reads as "busy".

## ServiceConnectionOverlay

The modal "disconnected" gate: full-screen overlay with a spinner, title, free-text detail
(e.g. a retry-attempt counter), optional extra content, and the embedded `ServiceStateUi`
tree — so the operator sees *what* is wrong before any functional test. Visibility derives
from the tree's aggregate state, overridable for hosts that track connectedness separately:

```razor
<ServiceConnectionOverlay DataContext="_client.ServiceState"
                          Title="Disconnected from Soundling service"
                          Detail="@RetryDetail"
                          VisibleOverride="@(!_client.IsConnected)" />
```

Both components expose `gsp-*` CSS hooks for app-level styling adjustments.

## House style

The GameshowPro look (Suede's quiet neutral surfaces, Plus Jakarta Sans and Cascadia Code, light and dark
following the operating system), shared by the products' web UIs. Everything is self-hosted, so it works offline and
from a device's flash. A product contributes only its accent.

In `index.html`, in the order shown (the preloads are optional but avoid a flash of fallback text):

```html
<link rel="preload" href="_content/GameshowPro.Common.Blazor/fonts/PlusJakartaSans-Variable.woff2" as="font" type="font/woff2" crossorigin>
<link rel="preload" href="_content/GameshowPro.Common.Blazor/fonts/CascadiaCode.woff2" as="font" type="font/woff2" crossorigin>
<link rel="stylesheet" href="_content/GameshowPro.Common.Blazor/fonts/fonts.css" />
<link rel="stylesheet" href="_content/GameshowPro.Common.Blazor/gsp-house.css" />
```

In the layout, with the product's accent:

```razor
<GspThemeProvider Accent="MyApp.Accent" />   @* plus MudDialogProvider, MudSnackbarProvider, ... *@
<GspAppHeader IconUrl="images/icon.svg" ProductName="MyApp" Version="1.2.3" VersionTooltip="Built 2026-11-19">
    <span class="gsp-pill ok">Connected</span>
</GspAppHeader>
<main class="gsp-main">
    <section class="gsp-panel">
        <h2 class="gsp-eyebrow">Service</h2>
        ...
    </section>
</main>
<GspPageFooter ProductName="MyApp" Version="1.2.3" />
```

- **`GspThemeProvider`** wraps `MudThemeProvider` with `GspHouseTheme.Create(accent)`. It follows the OS light/dark
  setting at startup and live; `ForceDarkMode` pins a mode and `IsDarkModeChanged` reports the effective one.
- **`GspAppHeader`** is the sticky header: icon, product name (the page's `h1`), a muted monospace version pill (with
  an optional tooltip), and a right-hand area for its child content that wraps below the brand on narrow screens.
- **`GspPageFooter`** is the text footer: `Product version, © 2026 Barjonas LLC`, plus optional child content.
- **Classes**: `gsp-main` (centered column, `--gsp-main-max-width`, default 940px), `gsp-panel`, `gsp-eyebrow`
  (section label), `gsp-pill` with `ok`, `warn`, `bad` or `muted` (color a status only together with a word or an
  icon), `gsp-data` (numbers, timestamps, identifiers: mono, tabular digits, slashed zero, no ligatures),
  `gsp-data-field` (the same on a MudBlazor input), `gsp-code` (mono with ligatures).
- **Tokens** (CSS variables): `--gsp-bg`, `--gsp-panel`, `--gsp-sunken`, `--gsp-ink`, `--gsp-muted`, `--gsp-line`,
  `--gsp-ok`, `--gsp-warn`, `--gsp-bad`, `--gsp-shadow`, `--gsp-accent` (fill), `--gsp-accent-ink` (text on the fill),
  `--gsp-accent-text` (the accent as text or outline on the page); `--font-sans`, `--font-mono`. They follow the
  active MudBlazor palette, so a plain-CSS element and a `MudButton` always agree.

### Choosing an accent

A `GspAccent` has a light and a dark `GspAccentColors (Fill, OnFill, Text)`. `Fill` is the brand color, `OnFill` is
the text on it, and `Text` is the same hue used as text on the page, which must reach WCAG AA (4.5:1) on the
background, panel and sunken colors. When the fill is too light (or too dark) for that, use a darker (lighter) shade
of the same hue as `Text`. `GspAccent.Indigo` is Suede's. Guard your own accent as `HouseThemeContrastTests` does:
`GspContrast.Ratio` measures a pair. Before Blazor starts (a splash screen), override
`--gsp-default-accent`, `--gsp-default-accent-ink` and `--gsp-default-accent-text` in the page's own CSS so the splash
already wears the accent.
