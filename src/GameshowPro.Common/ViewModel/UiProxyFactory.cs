namespace GameshowPro.Common.ViewModel;

/// <summary>
/// Abstracts UI-thread dispatching so that model-layer code can marshal work onto the
/// correct thread without depending on a specific UI framework (WPF Dispatcher, Blazor
/// <c>InvokeAsync</c>, etc.).
/// </summary>
/// <remarks>
/// Built-in implementations:
/// <list type="bullet">
///   <item><description><see cref="SynchronizationContextDispatcher"/> — for Blazor Server, Blazor WebAssembly, and any context that provides a <see cref="SynchronizationContext"/>.</description></item>
///   <item><description><c>WpfDispatcher</c> (Windows builds only) — wraps a WPF <c>Dispatcher</c>.</description></item>
/// </list>
/// Custom implementations are also supported; implement this interface directly for
/// frameworks or scenarios not covered above.
/// </remarks>
public interface IUiThreadDispatcher
{
    /// <summary>
    /// Returns <see langword="true"/> when the calling thread is the UI thread (or Blazor
    /// synchronization context), allowing callers to skip dispatching when already on
    /// the correct thread.
    /// </summary>
    bool CheckAccess();

    /// <summary>
    /// Executes <paramref name="action"/> on the UI thread. If the caller is already on
    /// the UI thread, the action may execute synchronously (implementation-defined).
    /// </summary>
    void Invoke(Action action);

    /// <summary>
    /// Executes <paramref name="func"/> asynchronously on the UI thread and returns its
    /// result. Use this when the model must wait for the UI to process an update before
    /// proceeding, e.g. showing a dialog.
    /// </summary>
    Task<TResult> InvokeAsync<TResult>(Func<TResult> func);
}

/// <summary>
/// Wraps an <see cref="INotifyPropertyChanged"/> model and re-raises its
/// <see cref="INotifyPropertyChanged.PropertyChanged"/> events on the UI thread via
/// <see cref="SafePropertyChanged"/>.
/// </summary>
/// <remarks>
/// <para>
/// This enables singleton or shared model objects to be safely observed from any UI
/// context (WPF window, Blazor circuit, etc.) without the model itself needing to know
/// about the UI framework.
/// </para>
/// <para>
/// For custom events on the model that are not <see cref="INotifyPropertyChanged.PropertyChanged"/>,
/// subscribe directly on <see cref="Model"/> and use <see cref="Invoke"/> to marshal
/// handlers onto the UI thread manually.
/// </para>
/// </remarks>
/// <typeparam name="T">The model type. Must implement <see cref="INotifyPropertyChanged"/>.</typeparam>
public sealed class ObservableUiProxy<T> : IDisposable where T : INotifyPropertyChanged
{
    /// <summary>
    /// The underlying model instance. Bind against this for read access; subscribe to
    /// <see cref="SafePropertyChanged"/> (not <c>Model.PropertyChanged</c>) to receive
    /// change notifications on the UI thread.
    /// </summary>
    public T Model { get; }

    private readonly IUiThreadDispatcher _dispatcher;

    /// <summary>
    /// Raised on the UI thread whenever <see cref="Model"/> raises
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs>? SafePropertyChanged;

    /// <summary>
    /// Creates a new proxy that listens to <paramref name="model"/>'s property changes
    /// and re-raises them on the thread owned by <paramref name="dispatcher"/>.
    /// </summary>
    public ObservableUiProxy(T model, IUiThreadDispatcher dispatcher)
    {
        Model = model;
        _dispatcher = dispatcher;
        Model.PropertyChanged += OnModelPropertyChanged;
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_dispatcher.CheckAccess())
        {
            SafePropertyChanged?.Invoke(this, e);
        }
        else
        {
            Invoke(() => SafePropertyChanged?.Invoke(this, e));
        }
    }

    /// <summary>
    /// Unsubscribes from <see cref="Model"/>'s property-changed events. Always call this
    /// (or use <see langword="using"/>) when the proxy is no longer needed to prevent leaks.
    /// </summary>
    public void Dispose()
    {
        Model.PropertyChanged -= OnModelPropertyChanged;
    }

    /// <summary>
    /// Executes <paramref name="action"/> on the UI thread. Use this to marshal custom
    /// event handlers that are not covered by <see cref="SafePropertyChanged"/>.
    /// </summary>
    /// <param name="action">The delegate to invoke on the UI thread.</param>
    public void Invoke(Action action)
        => _dispatcher.Invoke(action);

    /// <summary>
    /// Executes <paramref name="func"/> asynchronously on the UI thread and returns the
    /// result. Use this when the caller must wait for the UI to process an update, e.g. dialogs.
    /// </summary>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="func">The function to execute on the UI thread.</param>
    /// <returns>A task whose result is the return value of <paramref name="func"/>.</returns>
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
        => _dispatcher.InvokeAsync(func);
}

/// <summary>
/// Creates <see cref="ObservableUiProxy{T}"/> instances that are bound to a specific
/// <see cref="IUiThreadDispatcher"/>. Register as scoped in DI so each Blazor circuit
/// (or WPF window) gets its own factory backed by the correct dispatcher.
/// </summary>
public interface IUiProxyFactory
{
    /// <summary>
    /// Wraps <paramref name="model"/> in a proxy that re-raises property-change events
    /// on the UI thread.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="model">The shared model instance to observe.</param>
    /// <returns>A new <see cref="ObservableUiProxy{T}"/> that must be disposed by the caller.</returns>
    ObservableUiProxy<T> Create<T>(T model) where T : INotifyPropertyChanged;
}

/// <summary>
/// Default implementation of <see cref="IUiProxyFactory"/>. Delegates dispatching to the
/// <see cref="IUiThreadDispatcher"/> provided at construction time.
/// </summary>
public sealed class UiProxyFactory(IUiThreadDispatcher dispatcher) : IUiProxyFactory
{
    private readonly IUiThreadDispatcher _dispatcher = dispatcher;

    /// <inheritdoc />
    public ObservableUiProxy<T> Create<T>(T model) where T : INotifyPropertyChanged
    {
        return new ObservableUiProxy<T>(model, _dispatcher);
    }
}
