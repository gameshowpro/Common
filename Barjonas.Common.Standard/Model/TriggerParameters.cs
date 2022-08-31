using System;

namespace Barjonas.Common.Model
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class TriggerParameters : Attribute
    {
        public string Name { get; }
        public byte DefaultId { get; }
        public TimeSpan DebounceInterval { get; }
        public TriggerFilter TriggerFilter { get; }
        [Obsolete("This overload is for backwards-compatability only. Specify a TriggerFilter instead.")]
        public TriggerParameters(string name, byte defaultId, bool executeOnFirstInterrupt)
            : this(name, defaultId, executeOnFirstInterrupt ? TriggerFilter.FirstOnly : TriggerFilter.All)
        {
        }

        public TriggerParameters(string name, byte defaultId, TriggerFilter triggerFilter = TriggerFilter.All, int debounceIntervalMs = 1000)
        {
            Name = name;
            DefaultId = defaultId;
            TriggerFilter = triggerFilter;
            DebounceInterval = TimeSpan.FromMilliseconds(debounceIntervalMs);
        }
    }
}
