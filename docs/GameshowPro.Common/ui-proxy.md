# ObservableUiProxy — Thread-Safe UI Binding for Shared Models

## Problem

Shared (often singleton) model objects raise `INotifyPropertyChanged` events on whatever thread mutates them. UI frameworks require those notifications on a specific thread:

| Framework | Requirement |
|---|---|
| **WPF** | WPF marshals scalar `INotifyPropertyChanged` bindings automatically, but custom event handlers and collection changes still need the dispatcher thread. |
| **Blazor Server** | Each circuit has its own `SynchronizationContext`. A singleton model cannot capture "the" UI context because there are many. Components must call `InvokeAsync` on their own context. |
| **Blazor WebAssembly** | Single-threaded today, but the pattern still applies for consistency and future-proofing. |

## Solution

`ObservableUiProxy<T>` sits between the model and the UI. It subscribes to the model's `PropertyChanged` event and re-raises it as `SafePropertyChanged` on the correct UI thread via an `IUiThreadDispatcher`.

```
┌─────────────┐  PropertyChanged ┌──────────────────┐  SafePropertyChanged  ┌────────────┐
│  Model (DI) │ ───────────────► │ ObservableUiProxy│ ────────────────────► │  UI / Page │
│  (singleton)│                  │   (scoped)       │   (on UI thread)      │  (scoped)  │
└─────────────┘                  └──────────────────┘                       └────────────┘
```

## Types

### `IUiThreadDispatcher`

Abstracts the target UI thread. The library ships two built-in implementations; custom implementations are also supported by implementing the interface directly.

| Member | Purpose |
|---|---|
| `bool CheckAccess()` | Returns `true` when the caller is already on the UI thread. |
| `void Invoke(Action)` | Executes an action on the UI thread. |
| `Task<T> InvokeAsync<T>(Func<T>)` | Executes a function on the UI thread and returns the result. Use for operations where the caller must wait, e.g. dialogs. |

#### Built-in implementations

| Class | Package | Use when |
|---|---|---|
| `SynchronizationContextDispatcher` | `GameshowPro.Common` | Blazor Server, Blazor WebAssembly, or any framework that provides a `SynchronizationContext`. Captures the context at construction time — register as **scoped**. |
| `WpfDispatcher` | `GameshowPro.Common.Windows` | WPF applications. Takes a `Dispatcher` parameter — typically `Application.Current.Dispatcher`. |

### `ObservableUiProxy<T>`

| Member | Purpose |
|---|---|
| `T Model` | The underlying model. Read properties from here. |
| `event SafePropertyChanged` | Raised on the UI thread whenever `Model.PropertyChanged` fires. |
| `void Invoke(Action)` | Manually marshal custom (non-INPC) event handlers onto the UI thread. |
| `Task<T> InvokeAsync<T>(Func<T>)` | Async equivalent for when the caller must await the result. |
| `void Dispose()` | Unsubscribes from the model. **Must be called** to prevent leaks. |

### `IUiProxyFactory` / `UiProxyFactory`

Factory registered in DI that creates proxies bound to the current scope's dispatcher.

## Setup

### Blazor Server / WebAssembly

```csharp
// Shared model — singleton, raises PropertyChanged from any thread.
builder.Services.AddSingleton<MyObservableObject>();

// Dispatcher + factory — scoped so each circuit gets its own.
builder.Services.AddScoped<IUiThreadDispatcher, SynchronizationContextDispatcher>();
builder.Services.AddScoped<IUiProxyFactory, UiProxyFactory>();
```

### WPF

```csharp
// Shared model.
services.AddSingleton<MyObservableObject>();

// WpfDispatcher wraps the WPF Dispatcher.
services.AddScoped<IUiThreadDispatcher>(_ => new WpfDispatcher(Application.Current.Dispatcher));
services.AddScoped<IUiProxyFactory, UiProxyFactory>();
```

### Custom dispatcher

If neither built-in implementation fits, implement `IUiThreadDispatcher` directly and register it the same way:

```csharp
builder.Services.AddScoped<IUiThreadDispatcher, MyCustomDispatcher>();
```

## Usage in a Blazor Component

```razor
@inject IUiProxyFactory ProxyFactory
@inject MyObservableObject DeviceManager
@implements IDisposable

<h2>@_proxy?.Model.Status</h2>

@if (_isBuzzerActive)
{
    <div class="buzzer-alert">BUZZER PRESSED!</div>
}

@code {
    private ObservableUiProxy<MyObservableObject>? _proxy;
    private bool _isBuzzerActive;

    protected override void OnInitialized()
    {
        _proxy = ProxyFactory.Create(DeviceManager);
        _proxy.SafePropertyChanged += OnPropertyChanged;

        // Custom events are NOT automatically marshalled.
        // Subscribe on the model, then use Invoke to jump to the UI thread.
        _proxy.Model.BuzzerTriggered += OnBuzzerTriggered;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Already on the UI thread — safe to call StateHasChanged directly.
        StateHasChanged();
    }

    private void OnBuzzerTriggered(object? sender, EventArgs e)
    {
        // Background thread — must marshal.
        _proxy!.Invoke(() =>
        {
            _isBuzzerActive = true;
            StateHasChanged();
        });
    }

    public void Dispose()
    {
        if (_proxy is not null)
        {
            _proxy.SafePropertyChanged -= OnPropertyChanged;
            _proxy.Model.BuzzerTriggered -= OnBuzzerTriggered;
            _proxy.Dispose();
        }
    }
}
```

## Key Points

- **One proxy per component per model.** The proxy's lifetime matches the component. Create it in `OnInitialized` (or `OnParametersSet` if the model can change) and dispose it in `Dispose`.
- **`SafePropertyChanged` is already on the UI thread.** Call `StateHasChanged()` directly — no further marshalling needed.
- **Custom events need manual marshalling.** Use `_proxy.Invoke(...)` or `_proxy.InvokeAsync(...)` inside handlers for events other than `PropertyChanged`.
- **Singletons must not capture a `SynchronizationContext`.** The proxy/factory pattern exists precisely so that the model stays context-agnostic while each UI client dispatches through its own scoped context.
- **Blazor WebAssembly** works identically. Today WASM is single-threaded so `CheckAccess()` always returns `true` and dispatching is a no-op, but the pattern prepares for multi-threaded WASM.
