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

The modal "disconnected" gate: full-screen overlay with a spinning ring (around the product logo, see "House style"), title, free-text detail
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

The GameshowPro look (Suede's quiet neutral surfaces, Plus Jakarta Sans and Cascadia Code, light and dark following the
operating system), shared by the products' web UIs. Everything is self-hosted, so it works offline and from a device's
flash. An app declares as little as possible: what the build already knows is derived, the rest is set once.

### Setup: four places

**1. `Program.cs`**, one registration (it also registers MudBlazor's services, so do not call `AddMudServices` separately):

```csharp
builder.Services.AddGspHouseStyle(o =>
{
    o.Accent = MyApp.Accent;                 // see "Choosing an accent"; optional o.SecondaryAccent
    o.Tagline = "What the product does, in one line.";
    o.LicenseUrl = "https://github.com/gameshowpro/MyApp/blob/main/LICENSE";
});
```

**2. The layout** (`MainLayout.razor`):

```razor
@inherits LayoutComponentBase
<GspAppShell>@Body</GspAppShell>
```

`GspAppShell` is the theme (following the OS), MudBlazor's providers, the header, the content column, the footer, and the
About dialog with its button. Its parameters (`HeaderContent`, `FooterContent`, `AboutContent`, `ForceDarkMode`) are optional.

**3. `index.html`**: one stylesheet, the loading screen and the error bar (see below). `gsp-house.css` brings MudBlazor's
stylesheet, the fonts and the house rules, so do not link `MudBlazor.min.css` as well (a second copy after it would override
the house rules). Link `gsp-house-upright.css` instead for a product served from a small device: it leaves out the two italic
font faces (about 225 KB) that nothing in the house style needs, so the browser never requests them; the files still ship in
the package, so a bundler can drop `*Italic*.woff2`.

```html
<link rel="stylesheet" href="_content/GameshowPro.Common.Blazor/gsp-house.css" />
```

**4. The icon**: `wwwroot/images/app-icon.svg` (the convention; `o.IconPath` changes it once). The header, the About dialog, the
connection overlay and the loading screen all use it.

### What is derived

`GspAppInfo` (injected; `GspAppInfo.Resolve` builds one) reads the app's entry assembly:

| Shown | From |
|---|---|
| Product name | `AssemblyProduct` (`<Product>`; the SDK defaults it to the assembly name) |
| Version in the header and footer | `AssemblyInformationalVersion` without the `+commit` suffix (nbgv writes it); the About table shows the full one |
| Copyright | `AssemblyCopyright` (`<Copyright>`), else `© 2026 Barjonas LLC` |
| Source link | `AssemblyMetadata("RepositoryUrl")`, which the SDK emits when the project sets `<RepositoryUrl>` |
| Build date, license, documentation | `AssemblyMetadata` entries `GspBuildDate`, `PackageLicenseExpression`, `PackageProjectUrl` / `GspDocumentationUrl`, which this package's `buildTransitive` targets file adds at build time |

Every one of them can be overridden in `AddGspHouseStyle`. A project that references Common.Blazor as a **project** (as
opposed to the package) does not get the targets file automatically; import it once:

```xml
<Import Project="path\to\Common\src\GameshowPro.Common.Blazor\build\GameshowPro.Common.Blazor.targets" />
```

### What only the app knows: `GspRuntimeState`

Inject it and set what you learn; the header, footer and About dialog re-render:

```csharp
State.HostName = "fm01-106";      // right side of the header, About table
State.Platform = "ESP32";         // header pill reads "ESP32 1.2.3", About table
State.Connection = RemoteServiceStates.Connected;  // header pill: Connected (ok) / Reconnecting = Warning (warn) / Disconnected (bad); null hides it
State.SetAboutRow("Inputs", "12");
State.RequestAbout();             // opens the About dialog (the pill, the footer link and the button already do)
```

### Components

All take their values from the registration; their parameters are optional overrides.

- **`GspAppShell`**: the whole frame (see above).
- **`GspThemeProvider`**: `MudThemeProvider` with the house theme and the product's accents. It follows the OS light/dark
  setting at startup and **live** (changing the OS setting, or Edge DevTools' emulation, needs no reload); `ForceDarkMode`
  pins a mode and then ignores the OS. `IsDarkModeChanged` reports the effective mode.
- **`GspAppHeader`**: sticky header with a 2px rule in the accent (`--gsp-header-rule` changes it): icon, name (the page's
  `h1`), a version pill that opens the About dialog, host name and connection pill, and `ChildContent` for more.
- **`GspPageFooter`**: `Product version, © 2026 Barjonas LLC` and an About link.
- **`GspAboutButton`**: the small round button fixed at the bottom left (`o.ShowAboutButton = false` hides it).
- **`GspAboutDialog`**: a native `<dialog>`: Escape and a click on the backdrop close it, focus returns to whatever opened
  it. It shows the logo, name and tagline, a table of the version, build date, host, platform and the app's extra rows, the
  license paragraph, links (Documentation, Source, License text and `o.ExtraLinks`, in new tabs with
  `rel="noopener noreferrer"`), the trademark and copyright line, the "A GameshowPro application" credit, and `ChildContent`.
- **`ServiceConnectionOverlay`** (below) uses the same icon and name.

### Loading screen and error bar (static `index.html`)

Before Blazor starts nothing can be injected, so this is plain markup, convention-based: the icon at `images/app-icon.svg`
and the product name are the only literals.

```html
<div id="app">
    <div class="gsp-loading" role="status">
        <div class="gsp-ring"><img class="gsp-ring-logo" src="images/app-icon.svg" alt="" /></div>
        <span class="gsp-loading-text">Loading MyApp...</span>
    </div>
</div>
<div id="blazor-error-ui" role="alert">
    <strong>Error</strong> An unhandled error has occurred.
    <a href="" class="reload">Reload</a>
    <button type="button" class="dismiss" aria-label="Dismiss">×</button>
</div>
```

The logo sits in a ring whose arc is the real load progress: Blazor sets `--blazor-load-percentage` (and
`--blazor-load-percentage-text`) on `:root` while it downloads the app, and the CSS draws it. Without that variable the arc
spins instead; under `prefers-reduced-motion` it does not spin (the progress is still shown). The splash is drawn before
Blazor writes the theme, so give it the accent as text, once, in the page:

```html
<style>
    :root { --gsp-default-accent-text: #7b6f00; }
    @media (prefers-color-scheme: dark) { :root { --gsp-default-accent-text: #ffe600; } }
</style>
```

### Connection overlay

`ServiceConnectionOverlay` (published earlier, parameters unchanged) now dims and blurs the page behind it and shows a centered
card: the product logo inside a spinning ring, the title, the detail, `ChildContent` and the `ServiceStateUi` tree. New
optional parameters `IconUrl` and `ProductName` default to the registration; with a product name and no `Title` it reads
"Reconnecting to {ProductName}...", and without any registration it keeps its old title ("Disconnected from service") and
shows the ring alone.

### CSS: classes and tokens

Classes: `gsp-main` (centered column, `--gsp-main-max-width`, default 940px), `gsp-panel`, `gsp-eyebrow` (section label),
`gsp-pill` with `ok`, `warn`, `bad` or `muted` (color a status only together with a word or an icon), `gsp-data` (numbers,
timestamps, identifiers: mono, tabular digits, slashed zero, no ligatures), `gsp-data-field` (the same on a MudBlazor input),
`gsp-code` (mono with ligatures), `gsp-button` (+ `primary`) and `gsp-hint` for plain markup, `gsp-dialog` (a native
`<dialog>` with the house dialog look). The MudBlazor dialog, overlay backdrop, tooltip, outlined button and keyboard focus
ring are restyled globally, so a `MudDialog` and a `GspAboutDialog` match.

Tokens (CSS variables): `--gsp-bg`, `--gsp-panel`, `--gsp-sunken`, `--gsp-ink`, `--gsp-muted`, `--gsp-line`, `--gsp-ok`,
`--gsp-warn`, `--gsp-bad`, `--gsp-shadow`, `--gsp-accent` (fill), `--gsp-accent-ink` (text on the fill), `--gsp-accent-text`
(the accent as text or outline on the page), and the same three for the second accent (`--gsp-accent-2`,
`--gsp-accent-2-ink`, `--gsp-accent-2-text`); `--font-sans`, `--font-mono`. They follow the active MudBlazor palette, so a
plain-CSS element and a `MudButton` always agree.

### Choosing an accent

A `GspAccent` has a light and a dark `GspAccentColors (Fill, OnFill, Text)`. `Fill` is the brand color, `OnFill` is the text
on it, and `Text` is the same hue used as text on the page, which must reach WCAG AA (4.5:1) on the background, panel and
sunken colors. When the fill is too light (or too dark) for that, use a darker (lighter) shade of the same hue as `Text`.
`GspAccent.Indigo` is Suede's. The first accent is MudBlazor's `Primary`, the second (`o.SecondaryAccent`, optional) is its
`Secondary`; without a second accent `Secondary` is the house neutral. Guard your own accents as `HouseThemeContrastTests`
does: `GspContrast.Ratio` measures a pair.
