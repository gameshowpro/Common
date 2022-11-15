using Newtonsoft.Json;

namespace Barjonas.Common.Model;

/// <summary>
/// A base class for all IncomingTriggersDevice settings class
/// </summary>
public abstract class IncomingTriggerDeviceSettingsBase : NotifyingClass
{
    protected IncomingTriggerDeviceSettingsBase(IncomingTriggerSettings incomingTriggerSettings)
    {
        TriggerSettings = incomingTriggerSettings;
    }

    [JsonProperty]
    public virtual IncomingTriggerSettings TriggerSettings { get; }
}
