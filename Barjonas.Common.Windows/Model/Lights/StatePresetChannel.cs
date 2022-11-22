// (C) Barjonas LLC 2018

namespace Barjonas.Common.Model.Lights;

/// <summary>
/// Wrapper for a channel level preset which includes iNotifyPropertyChanged
/// </summary>
public class StatePresetChannel : NotifyingClass
{
    public StatePresetChannel() : this(null)
    {

    }

    [JsonConstructor]
    public StatePresetChannel(FixtureChannelType? fixtureChannelType)
    {
        _fixtureChannelType = fixtureChannelType ?? new();
    }

    private byte _level;
    [JsonProperty, DefaultValue(0)]
    public byte Level
    {
        get { return _level; }
        set { SetProperty(ref _level, value); }
    }

    private FixtureChannelType _fixtureChannelType;
    public FixtureChannelType FixtureChannelType
    {
        get { return _fixtureChannelType; }
        set { SetProperty(ref _fixtureChannelType, value); }
    }
}
