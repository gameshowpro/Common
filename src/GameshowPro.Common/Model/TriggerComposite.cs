#if WPF
using GameshowPro.Common.ViewModel;
#endif
namespace GameshowPro.Common.Model;

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
#if WPF
        SimulateTriggerCommand = new((toggle) => Triggered?.Invoke(this, new(toggle)));
#endif
    }

    private void RelayTrigger(object? sender, TriggerArgs args)
        => Triggered?.Invoke(sender, args);

#if WPF
    public RelayCommand<bool?> SimulateTriggerCommand { get; private set; }
#endif
}
