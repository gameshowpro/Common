namespace GameshowPro.Common.Model;
public enum DebounceMode
{
    [Description("Fire, block, wait")]
    FireBlockWait,
    [Description("Block, wait, fire")]
    BlockWaitFire
}

/// <summary>
/// Enforces a maximum invocation frequency of a single method by debouncing calls.
/// </summary>
/// <typeparam name="TArg">The argument type passed to the execution callback.</typeparam>
/// <remarks>Docs added by AI.</remarks>
public class Debouncer<TArg>
{
    private TArg? _latestArg;
    private DebounceMode Mode { get; }
    private readonly Timer _blockTimer;
    private bool _isBlocking = false;
    private bool _executeAfterBlock = false;
    /// <summary>
    /// Raised when the debouncer decides to execute the action with the latest argument.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public event EventHandler<TArg>? Execute;
    /// <summary>
    /// Initializes a new debouncer.
    /// </summary>
    /// <param name="mode">The debounce mode determining when to fire relative to the blocking interval.</param>
    /// <param name="minimumInterval">The minimum time between two executions.</param>
    /// <remarks>Docs added by AI.</remarks>
    public Debouncer(DebounceMode mode, TimeSpan minimumInterval)
    {
        _blockTimer = new Timer(TimerComplete, null, Timeout.Infinite, Timeout.Infinite);
        Mode = mode;
        MinimumInterval = minimumInterval;
    }

    /// <summary>
    /// Gets the minimum time enforced between two executions.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public TimeSpan MinimumInterval { get; }

    /// <summary>
    /// Will either raise the <see cref="Execute"/> event immediately or after a maximum delay of <see cref="MinimumInterval"/>, depending on time elapsed since last execution and <see cref="Mode"/>.
    /// </summary>
    /// <param name="arg">The argument to pass to the execution callback.</param>
    /// <remarks>Docs added by AI.</remarks>
    public void TryExecute(TArg arg)
    {
        lock (this)
        {
            _latestArg = arg;
            if (_isBlocking)
            {
                _executeAfterBlock = true;
            }
            else
            {
                _isBlocking = true;
                _blockTimer.Change(MinimumInterval, Timeout.InfiniteTimeSpan);
                if (Mode == DebounceMode.FireBlockWait)
                {
                    Execute?.Invoke(this, _latestArg);
                    _executeAfterBlock = false;
                }
                else
                {
                    _executeAfterBlock = true;
                }
            }
        }
    }

    private void TimerComplete(object? state)
    {
        lock (this)
        {
            //the blocking is over
            _isBlocking = false;
            _blockTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if (_executeAfterBlock)
            {
                _executeAfterBlock = false;
                if (_latestArg is not null)
                {
                    Execute?.Invoke(this, _latestArg);
                }
            }
        }
    }
}

