using System;
using System.Collections.ObjectModel;

namespace Barjonas.Common.Model
{
    public class IncomingTriggerSettings : KeyedCollection<string, IncomingTriggerSetting>
    {
        protected override string GetKeyForItem(IncomingTriggerSetting item)
        {
            return item.Key;
        }

        public IncomingTriggerSetting GetOrCreate(string key, string name, byte defaultId, bool executeOnFirstInterrupt = false, TimeSpan? debounceInterval = null)
        {
            if (name == null)
            {
                name = key;
            }
            IncomingTriggerSetting trigger;
            if (Contains(key))
            {
                trigger = this[key];
            }
            else
            {
                trigger = new IncomingTriggerSetting() { Key = key, Id = defaultId };
                Add(trigger);
            }
            trigger.Name = name;
            trigger.DebounceInterval = debounceInterval;
            trigger.ExecuteOnFirstInterrupt = executeOnFirstInterrupt;
            trigger._wasTouched = true;
            return trigger;
        }

        public void RemoveUntouched()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (!Items[i]._wasTouched)
                {
                    RemoveAt(i);
                }
            }
        }
    }
}
