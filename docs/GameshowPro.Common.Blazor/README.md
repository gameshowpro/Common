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
