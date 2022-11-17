using Newtonsoft.Json;

namespace Barjonas.Common.Model;

/// <summary>
/// A base class for all IncomingTriggersDevice settings classes
/// </summary>
public abstract class IncomingTriggerDeviceSettingsBase : NotifyingClass
{
    protected IncomingTriggerDeviceSettingsBase(IncomingTriggerSettings incomingTriggerSettings, bool? allowDuplicateTriggerIds)
    {
        TriggerSettings = incomingTriggerSettings;
        _allowDuplicateTriggerIds = allowDuplicateTriggerIds ?? false;
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

    [JsonProperty]
    public virtual IncomingTriggerSettings TriggerSettings { get; }
}
