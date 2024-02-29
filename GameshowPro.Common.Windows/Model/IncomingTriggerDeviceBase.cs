namespace GameshowPro.Common.Model;

/// <summary>
/// A base for <see cref="IncomingTriggerDevice{TTriggerKey, TTrigger}"/> which is common to all <see cref="IncomingTrigger"/> subclasses.
/// </summary>
public abstract class IncomingTriggerDeviceBase<TTriggerKey> : NotifyingClass, IIncomingTriggerDeviceBase
    where TTriggerKey : notnull, Enum
{
    private readonly PropertyChangeFilters _changeFilters = new();
    protected IncomingTriggerDeviceBase(
        string namePrefix,
        IncomingTriggerDeviceSettingsBase settings,
        ServiceState serviceState
    )
    {
        NamePrefix = namePrefix;
        ServiceState = serviceState;
        Settings = settings;
        _changeFilters.AddFilter((s, e) => AnyIsEnabled = settings.TriggerSettings.Any(s => s.IsEnabled), settings.TriggerSettings.Select(s => new PropertyChangeCondition(s, nameof(s.IsEnabled))));
    }

    public ServiceState ServiceState { get; }
    public string NamePrefix { get; }
    public IncomingTriggerDeviceSettingsBase Settings { get; }

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
    IncomingTriggerDeviceSettingsBase Settings { get; }
}
