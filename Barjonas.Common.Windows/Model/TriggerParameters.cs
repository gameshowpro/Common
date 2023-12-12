namespace Barjonas.Common.Model;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class TriggerParameters(string name, TriggerFilter triggerFilter = TriggerFilter.All, int debounceIntervalMs = 1000) : Attribute
{
    public string Name { get; } = name;
    public TimeSpan DebounceInterval { get; } = TimeSpan.FromMilliseconds(debounceIntervalMs);
    public TriggerFilter TriggerFilter { get; } = triggerFilter;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public sealed class TriggerDefaultSpecification<TParent>(int parentDeviceInstanceIndex, byte triggerId) : Attribute, ITriggerDefaultSpecification
    where TParent : IIncomingTriggerDeviceBase
{
    public int ParentDeviceInstanceIndex { get; } = parentDeviceInstanceIndex;
    public byte TriggerId { get; } = triggerId;
}

internal interface ITriggerDefaultSpecification
{
    public int ParentDeviceInstanceIndex { get; }
    public byte TriggerId { get; }
}
