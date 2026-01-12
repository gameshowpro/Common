#if WPF
using GameshowPro.Common.ViewModel;
#endif
namespace GameshowPro.Common.Model;

/// <summary>
/// A trigger which composes multiple child triggers into a single object.
/// </summary>
/// <summary>
/// Composes multiple triggers and forwards their events as a single trigger.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public class TriggerComposite : ITrigger
{
    /// <summary>
    /// Raised when any child trigger fires.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public event EventHandler<TriggerArgs>? Triggered;

    /// <summary>
    /// Creates a composite that relays events from the provided source triggers.
    /// </summary>
    /// <param name="sourceTriggers">The triggers to compose.</param>
    /// <remarks>Docs added by AI.</remarks>
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

    /// <summary>
    /// Forwards the child trigger event to subscribers.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    private void RelayTrigger(object? sender, TriggerArgs args)
        => Triggered?.Invoke(sender, args);

#if WPF
    /// <summary>
    /// Simulates a trigger event with an optional toggle value.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public RelayCommand<bool?> SimulateTriggerCommand { get; }
#endif
}
