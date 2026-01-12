#if WPF
using GameshowPro.Common.ViewModel;
#endif
namespace GameshowPro.Common.Model;

/// <summary>
/// A proxy which can be referenced from a model (e.g. contestant objects) allowing the trigger implementation to be changed dynamically (e.g. depending when user requests switch to a different source)
/// </summary>
public class IncomingTriggerProxy<TKey> : ObservableClass, ITrigger
    where TKey : notnull
{
    /// <summary>
    /// Available trigger options keyed by <typeparamref name="TKey"/>.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public FrozenDictionary<TKey, IncomingTrigger> Options { get; }

    /// <summary>
    /// Raised when the current source trigger fires.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public event EventHandler<TriggerArgs>? Triggered;

    /// <summary>
    /// Creates a proxy for a set of incoming triggers keyed by <typeparamref name="TKey"/>.
    /// </summary>
    /// <param name="options">Mapping of keys to triggers.</param>
    /// <remarks>Docs added by AI.</remarks>
    public IncomingTriggerProxy(FrozenDictionary<TKey, IncomingTrigger> options)
    {
        Options = options;
#if WPF
        SimulateTriggerCommand = new((toggle) => Triggered?.Invoke(this, new(toggle)));
#endif
    }

    private bool _firstSetIsDone = false; //If TKey is an enum, the source key will already be set to default, but the source will not be set
    private TKey? _currentSourceKey;
    /// <summary>
    /// Gets or sets the key for the active source trigger. Setting this updates <see cref="Source"/>.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    [DisallowNull]
    public TKey? CurrentSourceKey
    {
        get => _currentSourceKey;
        set
        {
            if ((SetProperty(ref _currentSourceKey, value) || !_firstSetIsDone) && Options.TryGetValue(value, out IncomingTrigger? newSource))
            {
                Source = newSource;
                _firstSetIsDone = true;
            }
        }
    }

    /// <summary>
    /// Attempts to retrieve an option by key.
    /// </summary>
    /// <param name="key">The key for the trigger.</param>
    /// <param name="option">Outputs the trigger when found.</param>
    /// <returns>True if found; otherwise false.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public bool TryGetOption(TKey key, out IncomingTrigger? option)
        => Options.TryGetValue(key, out option);

    private IncomingTrigger? _source;
    /// <summary>
    /// Gets the currently active source trigger.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public IncomingTrigger? Source
    {
        get => _source;
        private set
        {
            IncomingTrigger? previous = _source;
            if (SetProperty(ref _source, value))
            {
                if (previous is not null)
                {
                    previous.Triggered -= RelayTrigger;
                }
                if (_source is not null)
                {
                    _source.Triggered += RelayTrigger;
                }
            }
        }
    }

    private void RelayTrigger(object? sender, TriggerArgs args)
    {
        Triggered?.Invoke(sender, args);
    }
#if WPF
    /// <summary>
    /// Simulates a trigger event with an optional toggle value.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public RelayCommand<bool?> SimulateTriggerCommand { get; }
#endif
}
