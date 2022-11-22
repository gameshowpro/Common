// (C) Barjonas LLC 2018

using System.Threading;

namespace Barjonas.Common.Model.Lights;

/// <summary>
/// Represents a group of levels which go together to represent a particular state of a light.
/// </summary>
public class StateLevels : NotifyingClass
{
    internal event EventHandler<IReadOnlyList<StatePresetChannel>>? Flash;

    public StateLevels() : this(null, null, null)
    { }

    [JsonConstructor]
    public StateLevels(string? key, ImmutableList<StatePresetChannel>? levels, ImmutableList<StatePresetChannel>? flashLevels)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key), "Can't create StateLevels without key");
        Levels = levels ?? ImmutableList<StatePresetChannel>.Empty;
        FlashLevels = flashLevels ?? ImmutableList<StatePresetChannel>.Empty;
        _flashTimer = new Timer((o) => DoFlash());
    }

    [JsonProperty]
    public string Key { get; }

    [JsonProperty]
    public ImmutableList<StatePresetChannel> Levels { get; set; }

    public ImmutableList<StatePresetChannel> FlashLevels { get; }

    private float _flashOnDuration = 0;
    [JsonProperty, DefaultValue(0)]
    public float FlashOnDuration
    {
        get { return _flashOnDuration; }
        set { SetProperty(ref _flashOnDuration, value); }
    }

    private float _flashOffDuration = 0;
    [JsonProperty, DefaultValue(0)]
    public float FlashOffDuration
    {
        get { return _flashOffDuration; }
        set { SetProperty(ref _flashOffDuration, value); }
    }

    private int _flashCount = 0;
    [JsonProperty, DefaultValue(0)]
    public int FlashCount
    {
        get { return _flashCount; }
        set { SetProperty(ref _flashCount, value); }
    }

    private readonly Timer _flashTimer;

    private void DoFlash()
    {
        _flashIsOn = !_flashIsOn;
        if (_flashIsOn)
        {
            _flashCounter++;
        }
        Flash?.Invoke(this, _flashIsOn ? Levels : FlashLevels);
        if (_flashCount <= 0 || _flashCounter < _flashCount)
        {
            _flashTimer.Change(TimeSpan.FromSeconds(_flashIsOn ? _flashOnDuration : _flashOffDuration), Timeout.InfiniteTimeSpan);
        }
    }

    private bool _flashIsOn;
    private int _flashCounter;
    public void ResetFlash(bool enable)
    {
        if (enable)
        {
            _flashCounter = 0;
            _flashIsOn = false;
            DoFlash();
        }
        else
        {
            _flashTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
