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
        AllowDuplicateTriggerIds = allowDuplicateTriggerIds ?? false;
        NameSuffix = nameSuffix ?? string.Empty;
        IsEnabled = isEnabled ?? true;
        RemoteServiceSettings = remoteServiceSettings ?? new RemoteServiceSettings();
    }

    /// <summary>
    /// By default, triggers must have unique IDs. If this property is true, multiple triggers may share an ID,
    /// so that they will all be triggered when a remote trigger is received with that ID.
    /// </summary>
    [DataMember]
    public bool AllowDuplicateTriggerIds
    {
        get { return field; }
        set { SetProperty(ref field, value); }
    }

    /// <summary>
    /// A name which can be used shown on the UI to distinguish this device instance from another of the same type.
    /// </summary>
    [DataMember]
    public string NameSuffix
    {
        get { return field; }
        set { SetProperty(ref field, value); }
    } = "";

    [DataMember]
    public virtual IncomingTriggerSettings TriggerSettings { get; } = [];

    [DataMember]
    public bool IsEnabled
    {
        get => field;
        set => _ = SetProperty(ref field, value);
    } = true;
    [DataMember]
    public RemoteServiceSettings RemoteServiceSettings { get; } = new();
}
