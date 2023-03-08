using System.Collections.ObjectModel;

namespace Barjonas.Common.Model;

public class IncomingTriggerSettings : KeyedCollection<string, IncomingTriggerSetting>
{
    protected override string GetKeyForItem(IncomingTriggerSetting item)
    {
        return item.Key;
    }

    [Obsolete("This overload is for backwards-compatibility only. Specify a TriggerFilter instead of executeOnFirstInterrupt.")]
    public IncomingTriggerSetting GetOrCreate(string key, string name, byte? defaultId, bool executeOnFirstInterrupt = false, TimeSpan? debounceInterval = null)
        => GetOrCreate(key, name, defaultId, executeOnFirstInterrupt ? TriggerFilter.FirstOnly : TriggerFilter.All, debounceInterval);


    public virtual IncomingTriggerSetting GetOrCreate(string key, string name, byte? defaultId, TriggerFilter triggerFilter, TimeSpan? debounceInterval = null)
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
            trigger = new IncomingTriggerSetting()
            {
                Key = key,
                Id = defaultId ?? 127,
                Name = name,
                DebounceInterval = debounceInterval,
                TriggerEdge = true,
                IsEnabled = defaultId.HasValue
            };
            Add(trigger);
        }
        trigger.TriggerFilter = triggerFilter; //not user-configurable for now
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
