using System;

namespace Barjonas.Common.Model
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class TriggerParameters : Attribute
    {
        public readonly string _name;
        public readonly byte _defaultId;
        public readonly bool _executeOnFirstInterrupt;
        public TriggerParameters(string name, byte defaultId, bool executeOnFirstInterrupt = false)
        {
            _name = name;
            _defaultId = defaultId;
            _executeOnFirstInterrupt = executeOnFirstInterrupt;
        }
    }
}
