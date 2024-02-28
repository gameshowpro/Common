// (C) Barjonas LLC 2018

using GameshowPro.Common.ViewModel;

namespace GameshowPro.Common.Model.Lights;

/// <summary>
/// Represents a group of levels which go together to represent a particular state of a light.
/// </summary>
public class StateLevels : NotifyingClass
{
    internal event EventHandler<IReadOnlyList<StatePresetChannel>>? Flash;

    public StateLevels() : this(null, null, null, null)
    { }

    /// <summary>
    /// Create a <see cref="StateLevels"/> without any flashing.
    /// </summary>
    /// <param name="key">The name by which this StateLevels will be known within the <see cref="StatePresetGroup"/></param>
    /// <param name="levels">The levels to use when this <see cref="StateLevels"/> is applied.</param>
    public StateLevels(
       string? key,
       ImmutableList<StatePresetChannel>? levels
    )
    : this(key, ImmutableList.Create(new StateLevelsPhase(levels, null)), null, null)
    { }


    /// <summary>
    /// JSON constructor for <see cref="StateLevels"/> which includes possible legacy properties from previous versions.
    /// </summary>
    /// <param name="key">The name by which this StateLevels will be known within the <see cref="StatePresetGroup"/></param>
    /// <param name="levels">The levels to use during the "on" part or the flashing cycle.</param>
    /// <param name="flashLevels">The levels to use during the "off" part of the flashing cycle.</param>
    /// <param name="flashOnDuration">Time duration to wait after setting on color.</param>
    /// <param name="flashOffDuration">Time duration to wait after setting off color.</param>
    /// <param name="flashCount">The number of times to cycle between on and off. Zero or null denotes unlimited. This number is double to calculate the <see cref="CycleStepCount"/></param>
    [JsonConstructor, Obsolete("For JSON use only")]
    public StateLevels(
        string? key,
        IList<StateLevelsPhase>? phases,
        int? cycleStepCount,
        ImmutableList<StatePresetChannel>? levels,
        ImmutableList<StatePresetChannel>? flashLevels,
        float? flashOnDuration,
        float? flashOffDuration,
        int? flashCount
    ) : this(
        key,
        phases ?? (levels == null ? null : flashOnDuration > 0 && flashOffDuration > 0 ?
            [
                new StateLevelsPhase(levels, TimeSpan.FromSeconds(flashOnDuration.Value)),
                new StateLevelsPhase(flashLevels, TimeSpan.FromSeconds(flashOffDuration.Value)),
            ] :
            ImmutableList.Create(
                new StateLevelsPhase(levels, TimeSpan.Zero)
            )
        ),
        cycleStepCount ?? (flashCount ?? 0) * 2, 0)
    {
    }

    /// <summary>
    /// Create a <see cref="StateLevels"/> with all properties explicitly defined or defaulted.
    /// </summary>
    /// <param name="key">The name by which this StateLevels will be known within the <see cref="StatePresetGroup"/></param>
    /// <param name="phases">A list of all phases of this <see cref="StateLevels"/>, each of which can have a duration associated with it, in case cycling is required.</param>
    /// <param name="cycleStepCount">The number of times to cycle between the phases of this <see cref="StateLevels"/>. Zero or null denotes unlimited. The final color is dictated by <paramref name="cycleStepCount"/> modulus <paramref name="phases"/>.Count.</param>
    public StateLevels(
        string? key,
        IList<StateLevelsPhase>? phases,
        int? cycleStepCount,
        int? loopBackStep
    )
    {
        Key = key ?? throw new ArgumentNullException(nameof(key), "Can't create StateLevels without key");
        Phases = new(phases == null || !phases.Any() ? ImmutableList.Create(new StateLevelsPhase()) : phases);
        
        _cycleStepCount = cycleStepCount ?? 0;
        _loopBackStep = loopBackStep ?? 0;
        _flashTimer = new Timer((o) => DoFlash());
        AddPhaseCommand = new RelayCommandSimple(
            () => Phases.Add(
                new(Phases.Last().Levels.Select(l => new StatePresetChannel(l.FixtureChannelType)).ToImmutableList(),
                TimeSpan.FromSeconds(1)))
        );
        RemovePhaseCommand = new RelayCommandSimple(
            () =>
            {
                if (Phases.Count > 1)
                {
                    Phases.RemoveAt(Phases.Count - 1);
                }
            }
        );
        SetPhaseCyclingIsEnabled();
        Phases.CollectionChanged += (s, e) => SetPhaseCyclingIsEnabled();
        Phases.ItemPropertyChanged += (s, e) => { if (e.PropertyName == nameof(StateLevelsPhase.Duration)) { SetPhaseCyclingIsEnabled(); } };
        PropertyChanged += (s, e) => { if (e.PropertyName == nameof(CycleStepCount)) { SetPhaseCyclingIsEnabled(); } };

    }

    [JsonProperty]
    public string Key { get; }

    [JsonProperty]
    public ObservableCollectionEx<StateLevelsPhase> Phases { get; }


    private int _cycleStepCount = 0;
    [JsonProperty, DefaultValue(0)]
    public int CycleStepCount
    {
        get { return _cycleStepCount; }
        set { SetProperty(ref _cycleStepCount, value); }
    }

    private int _loopBackStep;
    [JsonProperty]
    public int LoopBackStep
    {
        get => _loopBackStep;
        set => _ = SetProperty(ref _loopBackStep, value);
    }

    private void SetPhaseCyclingIsEnabled()
    {
        HasMultiplePhases = Phases.Count > 1;
        PhaseCyclingIsEnabled = _hasMultiplePhases && _cycleStepCount != 1 && Phases.Count(p => p.Duration > TimeSpan.Zero) > 1;
        RemovePhaseCommand.SetCanExecute(_hasMultiplePhases);
    }

    private bool _phaseCyclingIsEnabled;
    [JsonIgnore]
    public bool PhaseCyclingIsEnabled
    {
        get => _phaseCyclingIsEnabled;
        private set => _ = SetProperty(ref _phaseCyclingIsEnabled, value);
    }

    private bool _hasMultiplePhases;
    [JsonIgnore]
    public bool HasMultiplePhases
    {
        get => _hasMultiplePhases;
        private set => _ = SetProperty(ref _hasMultiplePhases, value);
    }

    private readonly Timer _flashTimer;

    private void DoFlash()
    {
        StateLevelsPhase thisPhase = Phases[_cycleStep];
        _cycleStepCounter++;
        if ((_cycleStep + 1) >= Phases.Count)
        {
            _cycleStep = _loopBackStep.KeepInRange(0, Phases.Count - 1);
        }
        else
        {
            _cycleStep++;
        }
        Flash?.Invoke(this, thisPhase.Levels);
        if (_cycleStepCount <= 0 || _cycleStepCounter < _cycleStepCount)
        {
            if (thisPhase.Duration > TimeSpan.Zero)
            {
                _flashTimer.Change(thisPhase.Duration, Timeout.InfiniteTimeSpan);
            }
        }
    }

    private int _cycleStepCounter;
    private int _cycleStep;
    public void ResetFlash(bool enable)
    {
        if (enable)
        {
            _cycleStepCounter = 0;
            _cycleStep = 0;
            DoFlash();
        }
        else
        {
            _flashTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public RelayCommandSimple AddPhaseCommand { get; }
    public RelayCommandSimple RemovePhaseCommand { get; }
}
