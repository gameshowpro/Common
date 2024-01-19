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

    [JsonProperty]
    public ImmutableList<StatePresetChannel> Levels { get; internal set; }

    private TimeSpan _duration;
    [JsonProperty]
    public TimeSpan Duration
    {
        get => _duration;
        set => _ = SetProperty(ref _duration, value);
    }
}
