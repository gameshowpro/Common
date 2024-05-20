namespace GameshowPro.Common.Model.Lights;

public class StateLevelsPhase : NotifyingClass
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

    private TimeSpan _duration;
    [DataMember]
    public TimeSpan Duration
    {
        get => _duration;
        set => _ = SetProperty(ref _duration, value);
    }
}
