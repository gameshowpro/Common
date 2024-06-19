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
        _channelColors = channelColors ?? [];
        Validate();
    }

    [DataMember]
    public StatesLevels StatesLevels { get; }

    [Obsolete("Use StateLevels[key]?.Levels instead")]
    public IEnumerable<StatePresetChannel>? LevelByKey(string key)
    {
        if (StatesLevels.TryGetValue(key, out StateLevels? levels))
        {
            return levels.Phases.FirstOrDefault()?.Levels;
        }
        else
        {
            return null;
        }
    }

    private ImmutableList<FixtureChannelType> _channelColors;
    [DataMember]
    public ImmutableList<FixtureChannelType> ChannelColors
    {
        get
        {
            return _channelColors;
        }
        set
        {
            _channelColors = value;
            Validate();
        }
    }

    private void Validate()
    {
        int primCount = _channelColors.Count;
        foreach (StateLevels v in StatesLevels)
        {
            foreach (StateLevelsPhase p in v.Phases)
            {
                if (p.Levels.Count != primCount)
                {
                    p.Levels = p.Levels.EnsureListCount(primCount, primCount, (i) => new(_channelColors[i]));
                }
            }
        }
    }
}
