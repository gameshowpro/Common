using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Barjonas.Common.ViewModel;

namespace Barjonas.Common.Model;

public class IncomingTriggerComposite : IncomingTrigger
{
    private readonly ImmutableList<IncomingTrigger> _sourceTriggers;
    public IncomingTriggerComposite(IEnumerable<IncomingTrigger> sourceTriggers, string key, string name)
        : base(new IncomingTriggerSetting()
        {
            Name = name,
            Key = key,
            IsEnabled = false //Prevent base class from raising its own trigger events
        })
    {
        _sourceTriggers = sourceTriggers.ToImmutableList();
        foreach (IncomingTrigger trigger in sourceTriggers)
        {
            trigger.Triggered += RelayTriggered;
            trigger.IsDownChanged += (s, e) => UpdateIsDown();
        }
        UpdateIsDown();
    }


    private void UpdateIsDown()
    {
        IsDown = _sourceTriggers.Any(t => t.IsDown == t.Setting.TriggerEdge);
    }

}
