using GameshowPro.Common.ViewModel;

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

        SimulateTriggerCommand = new((toggle) => Triggered?.Invoke(this, new(toggle)));
    }

    private void RelayTrigger(object? sender, TriggerArgs args)
        => Triggered?.Invoke(sender, args);


    public RelayCommand<bool?> SimulateTriggerCommand { get; private set; }
}
