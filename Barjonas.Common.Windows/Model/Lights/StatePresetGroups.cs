// (C) Barjonas LLC 2018

using System.Collections.ObjectModel;

namespace Barjonas.Common.Model.Lights;

public class StatePresetGroups : KeyedCollection<string, StatePresetGroup>
{
    protected override string GetKeyForItem(StatePresetGroup item)
    {
        return item.Name;
    }

    /// <summary>
    /// Assign the assocuated FixtureChannelType to each StatePresetChannel so that the view can show the correct color highlighting for each channel.
    /// </summary>
    public void AddChannelTypes()
    {
        foreach (StatePresetGroup group in this)
        {
            foreach (StateLevels levels in group.StatesLevels)
            {
                int i = 0;
                foreach (StatePresetChannel chan in levels.Levels)
                {
                    chan.FixtureChannelType = group.ChannelColors[i];
                    i++;
                }
            }
        }
    }
}
