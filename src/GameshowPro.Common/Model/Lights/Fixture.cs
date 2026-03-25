// (C) Barjonas LLC 2018

#if WPF
using Color = System.Windows.Media.Color;
#endif

namespace GameshowPro.Common.Model.Lights;


/// <summary>
/// Represents a single logical light fixture.  This will contain one or more channels, e.g. different colors.  Each State represents values a fixed set of values for these channels.
/// </summary>
public class Fixture : ObservableClass
{
    public event Action? FlashStarting;
    public event Action? FlashEnded;
    [JsonConstructor]
    public Fixture(
        string key,
        string displayName,
        int startId,
        List<FixtureChannel> channels,
        StatePresetGroup group)
    {
        Key = key;
        DisplayName = displayName;
        StartId = startId;
        Channels = channels;
        StateGroup = group;
        State = group.StatesLevels.First();
        SetChannelsFromStartChannel(StartId, Channels);
        DeserializationComplete();
    }

    public event EventHandler? ChannelIdsChanged;
    /// <summary>
    /// The group of states which may be applied to this fixture.
    /// </summary>
    public StatePresetGroup StateGroup { get; internal set; }
    public StateLevels? State
    {
        get;
        private set => SetProperty(ref field, value);
    }

    [DataMember, DefaultValue(null)]
    public string Key { get; }

    [DataMember, DefaultValue("Light")]
    public string DisplayName
    {
        get { return field; }
        set { _ = SetProperty(ref field, value); }
    }

    [DataMember, DefaultValue(0)]
    public int StartId
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
            {
                SetChannelsFromStartChannel(field, Channels);
            }
        }
    }

    [DefaultValue(null)]
    public List<FixtureChannel> Channels
    {
        get { return field; }
        set
        {
            RemoveChannelHandlers(field, Channel_PropertyChanged);
            _ = SetProperty(ref field, value);
            SetChannelsFromStartChannel(StartId, field);
            DeserializationComplete(); //Mainly for backwards compatibility. If really deserializing, this should be called explicitly later
        }
    }

    private void SetChannelsFromStartChannel(int start, IEnumerable<FixtureChannel> channels)
    {
        bool changed = false;
        if (channels != null)
        {
            foreach (FixtureChannel channel in channels)
            {
                if (channel.Id != start)
                {
                    channel.Id = start;
                    changed = true;
                }
                start++;
            }
        }
        if (changed)
        {
            ChannelIdsChanged?.Invoke(this, new EventArgs());
        }
    }

    public void ClearAll()
    {
        ApplyStatePreset((IEnumerable<StatePresetChannel>?)null);
    }

    public void ReapplyStatePreset()
    {
        ApplyStatePreset(State?.Phases.FirstOrDefault()?.Levels);
    }

    public void ApplyStatePreset(string presetKey)
    {
        if (StateGroup == null)
        {
            throw new InvalidOperationException($"Cannot apply preset before {nameof(StateGroup)} is set");
        }
        ApplyStatePreset(StateGroup.StatesLevels[presetKey]);
    }

    private void ApplyStatePreset(IEnumerable<StatePresetChannel>? preset)
    {
        var i = 0;
        foreach (FixtureChannel chan in Channels)
        {
            chan.Level = preset?.ElementAtOrDefault(i)?.Level ?? 0;
            i++;
        }
    }

    public void ApplyStatePreset(StateLevels stateLevels, bool flashNow = true)
    {
        if (StateGroup == null)
        {
            throw new InvalidOperationException($"Can't set a {nameof(State)} without a {nameof(StateGroup)} already set.");
        }
        ArgumentNullException.ThrowIfNull(stateLevels);

        if (!StateGroup.StatesLevels.Contains(stateLevels))
        {
            throw new ArgumentException($"Can't set a {nameof(State)} which does not belong to configured {nameof(StateGroup)}.");
        }

        bool changed = State != stateLevels;
        if (changed)
        {
            if (State != null)
            {
                State.Flash -= CurrentState_Flash;
            }
            State = stateLevels;
            if (State != null)
            {
                State.Flash += CurrentState_Flash;
            }
        }
        if (changed)
        {
            if (stateLevels.PhaseCyclingIsEnabled)
            {
                if (flashNow)
                {
                    State?.ResetFlash(true);
                }
            }
            else
            {
                ApplyStatePreset(stateLevels.Phases.FirstOrDefault()?.Levels);
            }
        }
    }

    private void CurrentState_Flash(object? sender, IReadOnlyList<StatePresetChannel> e)
    {
        FlashStarting?.Invoke();
        ApplyStatePreset(e);
        FlashEnded?.Invoke();
    }

    public void DeserializationComplete()
    {
        if (Channels?.Any(c => c.FixtureChannelType == null) != false)
        {
            //Not ready yet
            return;
        }
        FixChannelIds();
        UpdateMixedColor();
        AddChannelHandlers(Channels, Channel_PropertyChanged, this);
    }

    private static void AddChannelHandlers(IList<FixtureChannel> channels, PropertyChangedEventHandler handler, Fixture fixture)
    {
        if (channels == null)
        { return; }
        foreach (FixtureChannel ch in channels)
        {
            ch.Parent = fixture;
            ch.PropertyChanged += handler;
            ch.FixtureChannelType.PropertyChanged += handler;
        }
    }

    private static void RemoveChannelHandlers(IList<FixtureChannel> channels, PropertyChangedEventHandler handler)
    {
        if (channels == null)
        { return; }
        foreach (FixtureChannel ch in channels)
        {
            ch.PropertyChanged -= handler;
            ch.FixtureChannelType.PropertyChanged -= handler;
        }
    }

    private void Channel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateMixedColor();
    }

    /// <summary>
    /// Correct any out-of-range channel IDs, updating their universe indices accordingly.
    /// </summary>
    private void FixChannelIds()
    {
        foreach (FixtureChannel channel in Channels)
        {
            if (channel.Id >= 512)
            {
                channel.UniverseIndex += Math.DivRem(channel.Id, 512, out int remainder);
                channel.Id = remainder;
            }
        }
    }

    private void UpdateMixedColor()
    {
        Color color = Colors.Black;
        foreach (FixtureChannel ch in Channels)
        {
            color += Color.Multiply(ch.FixtureChannelType.Primary, ch.Level / 255f);
        }
        MixedColor = color;
    }

    [DefaultValue("#00000000")]
    public Color MixedColor
    {
        get { return field; }
        private set { _ = SetProperty(ref field, value); }
    }
}
