// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Model.Lights;

public class StatePresetGroup
{
    [DataMember]
    public string Name { get; }

    [JsonConstructor]
    public StatePresetGroup(string name, ImmutableList<FixtureChannelType>? channelColors, StatesLevels? statesLevels)
    {
        Name = name;
        StatesLevels = statesLevels ?? [];
        ChannelColors = channelColors ?? [];
        Validate();
    }

    [DataMember]
    public StatesLevels StatesLevels { get; }

    [Obsolete("Use StateLevels[key]?.Levels instead")]
    public IEnumerable<StatePresetChannel>? LevelByKey(string key)
    {
        return StatesLevels.TryGetValue(key, out StateLevels? levels) ? (levels.Phases.FirstOrDefault()?.Levels) : (IEnumerable<StatePresetChannel>?)null;
    }

    [DataMember]
    public ImmutableList<FixtureChannelType> ChannelColors
    {
        get
        {
            return field;
        }
        set
        {
            field = value;
            Validate();
        }
    }

    private void Validate()
    {
        int primCount = ChannelColors.Count;
        foreach (StateLevels v in StatesLevels)
        {
            foreach (StateLevelsPhase p in v.Phases)
            {
                if (p.Levels.Count != primCount)
                {
                    p.Levels = p.Levels.EnsureListCount(primCount, primCount, (i) => new(ChannelColors[i]));
                }
            }
        }
    }
}
