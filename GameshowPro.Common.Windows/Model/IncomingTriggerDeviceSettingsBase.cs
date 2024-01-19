namespace GameshowPro.Common.Model;

/// <summary>
/// A base class for all IncomingTriggersDevice settings classes
/// </summary>
public abstract class IncomingTriggerDeviceSettingsBase : NotifyingClass
{
    protected IncomingTriggerDeviceSettingsBase(IncomingTriggerSettings incomingTriggerSettings, bool? allowDuplicateTriggerIds, string nameSuffix)
    {
        TriggerSettings = incomingTriggerSettings;
        _allowDuplicateTriggerIds = allowDuplicateTriggerIds ?? false;
        _nameSuffix = nameSuffix;
    }

    private bool _allowDuplicateTriggerIds;
    /// <summary>
    /// By default, triggers must have unique IDs. If this property is true, multiple triggers may share an ID,
    /// so that they will all be triggered when a remote trigger is received with that ID.
    /// </summary>
    [JsonProperty]
    public bool AllowDuplicateTriggerIds
    {
        get { return _allowDuplicateTriggerIds; }
        set { SetProperty(ref _allowDuplicateTriggerIds, value); }
    }

    private string _nameSuffix;
    /// <summary>
    /// A name which can be used shown on the UI to distinguish this device instance from another of the same type.
    /// </summary>
    [JsonProperty]
    public string NameSuffix
    {
        get { return _nameSuffix; }
        set { SetProperty(ref _nameSuffix, value); }
    }

    [JsonProperty]
    public virtual IncomingTriggerSettings TriggerSettings { get; }
}
