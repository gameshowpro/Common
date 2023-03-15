// (C) Barjonas LLC 2018

using Barjonas.Common.ViewModel;
namespace Barjonas.Common.Model;

public abstract class IncomingTrigger : NotifyingClass, ITrigger
{
    internal static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private readonly Stopwatch _lastTrigger = new();
    protected Logger Logger { get; } = LogManager.GetCurrentClassLogger();

    public delegate void IsDownChangedEventHandler(IncomingTrigger sender, bool isDown);
    /// <summary>
    /// Event raised after PropertyChanged for change to IsDown state, saving the consumer the overhead of checking the property name and value.
    /// </summary>
    public event IsDownChangedEventHandler? IsDownChanged;

    public event EventHandler<TriggerArgs>? Triggered;

    protected IncomingTrigger(IncomingTriggerSetting setting, IIncomingTriggerDeviceBase? parentDevice)
    {
        ParentDevice = parentDevice;
        _lastTrigger.Start();
        Setting = setting;
        SimulateTriggerCommand = new RelayCommand<bool?>((latch) => { if (latch == true) { IsDown = !_isDown; } else { OnConfiguredEdge(); } });
        ToggleIsEnabledCommand = new(() => { Setting.IsEnabled = !Setting.IsEnabled; });
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

    protected bool _isDown;
    public virtual bool IsDown
    {
        get { return _isDown; }
        protected set
        {
            if (SetProperty(ref _isDown, value))
            {
                if (ParentDevice != null)
                {
                    s_logger.Trace("Device {Device} trigger {id}/{trigger} IsDown changed to {value}", ParentDevice.NamePrefix, Setting.Id, Setting.Name, value);
                }
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
        => _lastTrigger.Elapsed > Setting.DebounceInterval;

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

    private int? _ordinal;
    /// <summary>
    /// The ordinal which was specified by the most recent triggerer.
    /// </summary>
    public int? Ordinal
    {
        get { return _ordinal; }
        set { SetProperty(ref _ordinal, value); }
    }

    private TimeSpan? _time;
    /// <summary>
    /// The time which was specified by the most recent triggerer.
    /// </summary>
    public TimeSpan? Time
    {
        get { return _time; }
        set { SetProperty(ref _time, value); }
    }

    private bool _isTest;
    /// <summary>
    /// The IsTest decaration which was specified by the most recent triggerer.  Primarily intended for to help log clarification.
    /// </summary>
    public bool IsTest
    {
        get { return _isTest; }
        set { SetProperty(ref _isTest, value); }
    }

    private DateTime _lastTriggerDateTime = DateTime.MinValue;
    public DateTime LastTriggerDateTime
    {
        get { return _lastTriggerDateTime; }
        protected set { SetProperty(ref _lastTriggerDateTime, value); }
    }

    private DateTime _lastLockoutDateTime = DateTime.MinValue;
    public DateTime LastLockoutDateTime
    {
        get { return _lastLockoutDateTime; }
        protected set { SetProperty(ref _lastLockoutDateTime, value); }
    }

    public RelayCommand<bool?> SimulateTriggerCommand { get; private set; }
    public RelayCommandSimple ToggleIsEnabledCommand { get; private set; }
}
