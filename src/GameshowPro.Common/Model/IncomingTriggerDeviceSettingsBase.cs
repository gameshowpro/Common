namespace GameshowPro.Common.Model;

/// <summary>
/// A base class for all IncomingTriggersDevice settings classes
/// </summary>
public abstract class IncomingTriggerDeviceSettingsBase : ObservableClass
{
    /// <summary>
    /// For deserialization purposes only.
    /// </summary>
    public IncomingTriggerDeviceSettingsBase()
    {
        TriggerSettings = [];
        RemoteServiceSettings = new();
    }

    public IncomingTriggerDeviceSettingsBase(IncomingTriggerSettings incomingTriggerSettings, bool? allowDuplicateTriggerIds, string nameSuffix, bool? isEnabled, RemoteServiceSettings? remoteServiceSettings)
    {
        TriggerSettings = incomingTriggerSettings ?? [];
        _allowDuplicateTriggerIds = allowDuplicateTriggerIds ?? false;
        _nameSuffix = nameSuffix ?? string.Empty;
        _isEnabled = isEnabled ?? true;
        RemoteServiceSettings = remoteServiceSettings ?? new RemoteServiceSettings();
    }

    private bool _allowDuplicateTriggerIds;
    /// <summary>
    /// By default, triggers must have unique IDs. If this property is true, multiple triggers may share an ID,
    /// so that they will all be triggered when a remote trigger is received with that ID.
    /// </summary>
    [DataMember]
    public bool AllowDuplicateTriggerIds
    {
        get { return _allowDuplicateTriggerIds; }
        set { SetProperty(ref _allowDuplicateTriggerIds, value); }
    }

    private string _nameSuffix = "";
    /// <summary>
    /// A name which can be used shown on the UI to distinguish this device instance from another of the same type.
    /// </summary>
    [DataMember]
    public string NameSuffix
    {
        get { return _nameSuffix; }
        set { SetProperty(ref _nameSuffix, value); }
    }

    [DataMember]
    public virtual IncomingTriggerSettings TriggerSettings { get; } = [];

    private bool _isEnabled = true;
    [DataMember]
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _ = SetProperty(ref _isEnabled, value);
    }

    [DataMember]
    public RemoteServiceSettings RemoteServiceSettings { get; } = new();
}
