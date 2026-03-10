namespace GameshowPro.Common.Model.Lights;

public class StateLevelsPhase : ObservableClass
{
    public StateLevelsPhase() : this(null, null)
    { }

    [JsonConstructor]
    public StateLevelsPhase(ImmutableList<StatePresetChannel>? levels, TimeSpan? duration)
    {
        Levels = levels ?? [];
        Duration = duration ?? TimeSpan.Zero;
    }

    [DataMember]
    public ImmutableList<StatePresetChannel> Levels { get; internal set; }
    [DataMember]
    public TimeSpan Duration
    {
        get;
        set => _ = SetProperty(ref field, value);
    }
}
