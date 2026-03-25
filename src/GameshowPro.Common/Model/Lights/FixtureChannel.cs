// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Model.Lights;

/// <summary>
/// Represents one channel within a <seealso cref="Fixture"/>.  For an RGB fixture it will be Red, Green or Blue.  For a single channel light, it could be anything.
/// </summary>
public class FixtureChannel : ObservableClass
{
    internal event EventHandler<byte>? LevelChanged;
    public FixtureChannel() : this(null) { }

    [JsonConstructor]
    public FixtureChannel(FixtureChannelType? fixtureChannelType)
    {
        FixtureChannelType = fixtureChannelType ?? new() { Primary = Colors.White };
    }

    public FixtureChannelType FixtureChannelType
    {
        get { return field; }
        set { SetProperty(ref field, value); }
    }

    [DataMember, DefaultValue(0)]
    public byte Level
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                LevelChanged?.Invoke(this, field);
            }
        }
    }

    [DataMember, DefaultValue(0)]
    public int Id
    {
        get;
        set { SetProperty(ref field, value); }
    }

    [DataMember, DefaultValue(0)]
    public int UniverseIndex
    {
        get;
        set { SetProperty(ref field, value); }
    } = 0;

    public Fixture? Parent
    {
        get;
        set { SetProperty(ref field, value); }
    }
}
