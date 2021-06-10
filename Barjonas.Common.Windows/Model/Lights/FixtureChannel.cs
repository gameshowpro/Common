// (C) Barjonas LLC 2018

using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Barjonas.Common.Model.Lights
{
    /// <summary>
    /// Represents one channel within a <seealso cref="Fixture"/>.  For an RGB fixture it will be Red, Green or Blue.  For a single channel light, it could be anything.
    /// </summary>
    public class FixtureChannel : NotifyingClass
    {
        internal event EventHandler<byte> LevelChanged;
        private FixtureChannelType _fixtureChannelType;
        public FixtureChannelType FixtureChannelType
        {
            get { return _fixtureChannelType; }
            set { SetProperty(ref _fixtureChannelType, value); }
        }

        private byte _level;
        [JsonProperty, DefaultValue(0)]
        public byte Level
        {
            get { return _level; }
            set
            {
                if (SetProperty(ref _level, value))
                {
                    LevelChanged?.Invoke(this, _level);
                }
            }
        }

        private int _id;
        [JsonProperty, DefaultValue(0)]
        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private int _universeIndex = 0;
        [JsonProperty, DefaultValue(0)]
        public int UniverseIndex
        {
            get { return _universeIndex; }
            set { SetProperty(ref _universeIndex, value); }
        }

        private Fixture _parent;
        public Fixture Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }
    }
}
