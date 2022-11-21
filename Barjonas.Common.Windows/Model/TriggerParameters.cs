using System;

namespace Barjonas.Common.Model;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class TriggerParameters : Attribute
{
    public string Name { get; }
    public TimeSpan DebounceInterval { get; }
    public TriggerFilter TriggerFilter { get; }

    public TriggerParameters(string name, TriggerFilter triggerFilter = TriggerFilter.All, int debounceIntervalMs = 1000)
    {
        Name = name;
        TriggerFilter = triggerFilter;
        DebounceInterval = TimeSpan.FromMilliseconds(debounceIntervalMs);
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public sealed class TriggerDefaultSpecification<TParent> : Attribute, ITriggerDefaultSpecification
    where TParent : IIncomingTriggerDeviceBase
{
    public int ParentDeviceInstanceIndex { get; }
    public byte TriggerId { get; }

    public TriggerDefaultSpecification(int parentDeviceInstanceIndex, byte triggerId)
    {
        ParentDeviceInstanceIndex = parentDeviceInstanceIndex;
        TriggerId = triggerId;
    }
}

internal interface ITriggerDefaultSpecification
{
    public int ParentDeviceInstanceIndex { get; }
    public byte TriggerId { get; }
}
