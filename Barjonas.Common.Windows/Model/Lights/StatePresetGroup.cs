// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Barjonas.Common.Model.Lights
{
    public class StatePresetGroup
    {
        public string Name { get; set; }

        private StatesLevels _statesLevels;
        [JsonProperty]
        public StatesLevels StatesLevels
        {
            get
            {
                return _statesLevels;
            }
            set
            {
                if (_statesLevels != null)
                {
                    throw new InvalidOperationException($"Once {nameof(StatesLevels)} is set, it is immutable.");
                }
                _statesLevels = value;
                Validate();
            }
        }

        [Obsolete("Use StateLevels[key]?.Levels instead")]
        public IEnumerable<StatePresetChannel> LevelByKey(string key)
        {
            if (StatesLevels.Contains(key))
            {
                return StatesLevels[key]?.Levels;
            }
            else
            {
                return null;
            }
        }

        private IReadOnlyList<FixtureChannelType> _channelColors;
        [JsonProperty]
        public IReadOnlyList<FixtureChannelType> ChannelColors
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
            if (_statesLevels != null && _channelColors != null)
            {
                var primCount = _channelColors.Count;
                foreach (StateLevels v in _statesLevels)
                {
                    if (v.Levels.Count != primCount)
                    {
                        throw new InvalidOperationException($"Number of levels in all {nameof(StateLevels)} object must match number of {nameof(ChannelColors)}.");
                    }
                }
            }
        }
    }
}
