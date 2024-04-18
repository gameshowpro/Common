namespace GameshowPro.Common.Model;

/// <summary>
/// A base for <see cref="IncomingTriggerDevice{TTriggerKey, TTrigger}"/> which is common to all <see cref="IncomingTrigger"/> subclasses.
/// </summary>
public abstract class IncomingTriggerDeviceBase<TTriggerKey> : NotifyingClass, IIncomingTriggerDeviceBase
    where TTriggerKey : notnull, Enum
{
    private readonly PropertyChangeFilters _changeFilters;
    protected ILoggerFactory _loggerFactory;
    protected ILogger _logger;
    protected IncomingTriggerDeviceBase(
        string namePrefix,
        int index,
        IncomingTriggerDeviceSettingsBase settings,
        ServiceState serviceState,
        ILoggerFactory loggerFactory
    )
    {
        _loggerFactory = loggerFactory;
        _changeFilters = new(GetType().Name);
        _logger = loggerFactory.CreateLogger(GetType());
        NamePrefix = namePrefix;
        Index = index;
        ServiceState = serviceState;
        Settings = settings;
        _changeFilters.AddFilter((s, e) => AnyIsEnabled = settings.TriggerSettings.Any(s => s.IsEnabled), settings.TriggerSettings.Select(s => new PropertyChangeCondition(s, nameof(s.IsEnabled))));
    }

    public ServiceState ServiceState { get; }
    public string NamePrefix { get; }
    public IncomingTriggerDeviceSettingsBase Settings { get; }
    public int Index { get; }

    /// <summary>
    /// A dictionary containing a list of all triggers belonging to this object, keyed by <see cref="TTriggerKey"/>, widely typed as <see cref="IncomingTrigger"/>.
    /// </summary>
    public abstract FrozenDictionary<TTriggerKey, IncomingTrigger> TriggersBase { get; }

    private bool _anyIsEnabled;
    /// <summary>
    /// True if any <see cref="IncomingTrigger"/> in <see cref="TriggersBase"/> is set to be enabled.
    /// </summary>
    public bool AnyIsEnabled
    {
        get => _anyIsEnabled;
        private set => _ = SetProperty(ref _anyIsEnabled, value);
    }
}

public interface IIncomingTriggerDeviceBase : IRemoteService
{
    string NamePrefix { get; }
    int Index { get; }
    IncomingTriggerDeviceSettingsBase Settings { get; }
}
