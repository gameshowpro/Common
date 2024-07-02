// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Model.Lights;

/// <summary>
/// Wrapper for a channel level preset which includes iNotifyPropertyChanged
/// </summary>
[method: JsonConstructor]
/// <summary>
/// Wrapper for a channel level preset which includes iNotifyPropertyChanged
/// </summary>
public class StatePresetChannel(FixtureChannelType? fixtureChannelType) : NotifyingClass
{
    public StatePresetChannel() : this(null)
    {

    }

    private byte _level;
    [DataMember, DefaultValue(0)]
    public byte Level
    {
        get { return _level; }
        set { SetProperty(ref _level, value); }
    }

    private FixtureChannelType _fixtureChannelType = fixtureChannelType ?? new();
    public FixtureChannelType FixtureChannelType
    {
        get { return _fixtureChannelType; }
        set { SetProperty(ref _fixtureChannelType, value); }
    }
}
