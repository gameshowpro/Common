namespace GameshowPro.Common.ViewModel;

/// <summary>
/// An <see cref="IUiThreadDispatcher"/> that marshals work onto the
/// <see cref="SynchronizationContext"/> captured at construction time.
/// </summary>
/// <remarks>
/// <para>
/// This is the recommended dispatcher for <strong>Blazor Server</strong> (where each
/// circuit has its own <see cref="SynchronizationContext"/>), <strong>Blazor
/// WebAssembly</strong>, and any other framework that exposes a synchronization
/// context. Register it as <em>scoped</em> so that each client receives a dispatcher
/// bound to its own context.
/// </para>
/// <para>
/// If the built-in behavior does not fit your scenario, implement
/// <see cref="IUiThreadDispatcher"/> directly instead.
/// </para>
/// </remarks>
public sealed class SynchronizationContextDispatcher : IUiThreadDispatcher
{
    private readonly SynchronizationContext _context = SynchronizationContext.Current
        ?? throw new InvalidOperationException(
            "No SynchronizationContext exists on the current thread. " +
            "Ensure this service is resolved on a thread that has a SynchronizationContext (e.g. inside a Blazor circuit).");

    /// <inheritdoc />
    public bool CheckAccess()
        => SynchronizationContext.Current == _context;

    /// <inheritdoc />
    public void Invoke(Action action)
    {
        if (CheckAccess())
        {
            action();
        }
        else
        {
            _context.Send(_ => action(), null);
        }
    }

    /// <inheritdoc />
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
    {
        if (CheckAccess())
        {
            return Task.FromResult(func());
        }

        TaskCompletionSource<TResult> tcs = new();
        _context.Post(_ =>
        {
            try { tcs.SetResult(func()); }
            catch (Exception ex) { tcs.SetException(ex); }
        }, null);
        return tcs.Task;
    }
}
