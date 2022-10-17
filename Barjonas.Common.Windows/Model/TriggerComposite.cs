using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Barjonas.Common.Model;

/// <summary>
/// A trigger which composes multiple child triggers into a single object.
/// </summary>
public class TriggerComposite : ITrigger
{
    public event EventHandler<TriggerArgs>? Triggered;
    public TriggerComposite(IEnumerable<ITrigger> sourceTriggers)
    {
        foreach (ITrigger trigger in sourceTriggers)
        {
            trigger.Triggered += RelayTrigger;
        }
    }

    private void RelayTrigger(object? sender, TriggerArgs args)
        => Triggered?.Invoke(sender, args);
}
