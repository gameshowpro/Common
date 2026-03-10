// (C) Barjonas LLC 2018

#if WPF
using GameshowPro.Common.ViewModel;
#endif
namespace GameshowPro.Common.Model;

public abstract class IncomingTrigger : ObservableClass, ITrigger
{
    protected readonly ILogger _logger;
    private readonly Stopwatch _lastTrigger = new();

    public delegate void IsDownChangedEventHandler(IncomingTrigger sender, bool isDown);
    /// <summary>
    /// Event raised after PropertyChanged for change to IsDown state, saving the consumer the overhead of checking the property name and value.
    /// </summary>
    public event IsDownChangedEventHandler? IsDownChanged;

    public event EventHandler<TriggerArgs>? Triggered;

    protected IncomingTrigger(IncomingTriggerSetting setting, IIncomingTriggerDeviceBase? parentDevice, ILogger logger)
    {
        _logger = logger;
        ParentDevice = parentDevice;
        _lastTrigger.Start();
        Setting = setting;
#if WPF
        SimulateTriggerCommand = new RelayCommand<bool?>((latch) => { if (latch == true) { IsDown = !IsDown; } else { OnConfiguredEdge(); } });
        ToggleIsEnabledCommand = new(() => { Setting.IsEnabled = !Setting.IsEnabled; });
#endif
    }

    public IIncomingTriggerDeviceBase? ParentDevice { get; }

    /// <summary>
    /// Called whenever <see cref="IsDown"/> changes value
    /// </summary>
    /// <param name="value"></param>
    protected void OnIsDownChanged(bool value)
    {
        IsDownChanged?.Invoke(this, value);

        if (BaseClassDetectsEdges && value == Setting.TriggerEdge)
        {
            OnConfiguredEdge();
        }
    }

    public IncomingTriggerSetting Setting { get; }

    public virtual bool IsDown
    {
        get { return field; }
        protected set
        {
            if (SetProperty(ref field, value))
            {
                _logger.LogTrace("IsDown changed to {value}", value);
                OnIsDownChanged(value);
            }
        }
    }

    public object? TriggerData { get; set; }

    /// <summary>
    /// Called by base class whenever IsDown edge is encountered that matches TriggerEdge setting and subclass has not suppressed it using TriggerWhenDown.
    /// Alternatively, could be called by base class.
    /// Trigger will not be passed on to RelayTriggered unless debounce and IsEnabled tests are passed
    /// </summary>
    protected void OnConfiguredEdge()
    {
        if (Setting.IsEnabled && !DebounceIsInProgress())
        {
            OnVerifiedTrigger(this, new TriggerArgs(TriggerData));
        }
    }

    protected bool DebounceIsInProgress()
        => _lastTrigger.Elapsed < Setting.DebounceInterval;

    protected void OnLockedOutEdge(TimeSpan lockoutTimeRemaining)
    {
        LastLockoutDateTime = DateTime.UtcNow;
        //Todo - use a timer to track how long the lockout lasts, to be surfaced on the UI
    }

    /// <summary>
    /// Called by base class whenever this trigger has been triggered. No additional filtering is performed in the base class.
    /// </summary>
    protected virtual void OnVerifiedTrigger(object? source, TriggerArgs args)
    {
        _lastTrigger.Restart();
        LastTriggerDateTime = DateTime.UtcNow;
        Triggered?.Invoke(source, args);
    }

    /// <summary>
    /// If true, <see cref="OnConfiguredEdge"/> will be called whenever configured edge is detected in <see cref="IsDown"/>.
    /// </summary>
    public virtual bool BaseClassDetectsEdges => true;

    /// <summary>
    /// The ordinal which was specified by the most recent triggerer.
    /// </summary>
    public int? Ordinal
    {
        get;
        set { SetProperty(ref field, value); }
    }

    /// <summary>
    /// The time which was specified by the most recent triggerer.
    /// </summary>
    public TimeSpan? Time
    {
        get;
        set { SetProperty(ref field, value); }
    }

    /// <summary>
    /// The IsTest decoration which was specified by the most recent triggerer.  Primarily intended for to help log clarification.
    /// </summary>
    public bool IsTest
    {
        get;
        set { SetProperty(ref field, value); }
    }

    public DateTime LastTriggerDateTime
    {
        get;
        protected set { SetProperty(ref field, value); }
    } = DateTime.MinValue;

    public DateTime LastLockoutDateTime
    {
        get;
        protected set { SetProperty(ref field, value); }
    } = DateTime.MinValue;
#if WPF
    public RelayCommand<bool?> SimulateTriggerCommand { get; }
    public RelayCommandSimple ToggleIsEnabledCommand { get; }
#endif
}
