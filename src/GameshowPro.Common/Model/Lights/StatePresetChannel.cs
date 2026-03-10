// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Model.Lights;

/// <summary>
/// Wrapper for a channel level preset which includes iNotifyPropertyChanged
/// </summary>
[method: JsonConstructor]
public class StatePresetChannel(FixtureChannelType? fixtureChannelType) : ObservableClass
{
    public StatePresetChannel() : this(null)
    {

    }

    [DataMember, DefaultValue(0)]
    public byte Level
    {
        get;
        set { SetProperty(ref field, value); }
    }

    public FixtureChannelType FixtureChannelType
    {
        get;
        set { SetProperty(ref field, value); }
    } = fixtureChannelType ?? new();
}
