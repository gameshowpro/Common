namespace GameshowPro.Common.Model;
public enum DebounceMode
{
    [Description("Fire, block, wait")]
    FireBlockWait,
    [Description("Block, wait, fire")]
    BlockWaitFire
}

/// <summary>
/// Class to enforce a maximum invocation frequency of single method.
/// </summary>
public class Debouncer<TArg>
{
    private TArg? _latestArg;
    private DebounceMode Mode { get; }
    private readonly Timer _blockTimer;
    private bool _isBlocking = false;
    private bool _executeAfterBlock = false;
    public event EventHandler<TArg>? Execute;
    public Debouncer(DebounceMode mode, TimeSpan minimumInterval)
    {
        _blockTimer = new Timer(TimerComplete, null, Timeout.Infinite, Timeout.Infinite);
        Mode = mode;
        MinimumInterval = minimumInterval;
    }

    public TimeSpan MinimumInterval { get; }

    /// <summary>
    /// Will either raise the <see cref="Execute"/> event immediately or after a maximum delay of <see cref="MinimumInterval"/>, depending on time elapsed since last execution and <see cref="Mode"/>.
    /// </summary>
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

