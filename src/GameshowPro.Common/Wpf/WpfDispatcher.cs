using GameshowPro.Common.ViewModel;

namespace GameshowPro.Common.Wpf;

/// <summary>
/// An <see cref="IUiThreadDispatcher"/> that delegates to a WPF
/// <see cref="Dispatcher"/>.
/// </summary>
/// <remarks>
/// <para>
/// Pass <see cref="Dispatcher.CurrentDispatcher"/> (or
/// <c>Application.Current.Dispatcher</c>) when registering in DI.
/// </para>
/// <para>
/// If the built-in behavior does not fit your scenario, implement
/// <see cref="IUiThreadDispatcher"/> directly instead.
/// </para>
/// </remarks>
public sealed class WpfDispatcher(Dispatcher dispatcher) : IUiThreadDispatcher
{
    /// <inheritdoc />
    public bool CheckAccess()
        => dispatcher.CheckAccess();

    /// <inheritdoc />
    public void Invoke(Action action)
        => dispatcher.Invoke(action);

    /// <inheritdoc />
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
        => dispatcher.InvokeAsync(func).Task;
}
