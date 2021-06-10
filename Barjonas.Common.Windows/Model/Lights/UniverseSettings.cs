// (C) Barjonas LLC 2018

using System.ComponentModel;
using Newtonsoft.Json;

namespace Barjonas.Common.Model.Lights
{
    public class UniverseSettings : NotifyingClass
    {
        private int _universeIndex = 1;
        /// <summary>
        /// Ultimately this gets sent out as a <see cref="short"/>, so value must be within signed 16-bit range.
        /// Also, it doesn't seem that many other devices support universes less than 1, so zero represents disable.
        /// </summary>
        [JsonProperty, DefaultValue(1)]
        public int UniverseIndex
        {
            get { return _universeIndex; }
            set { SetProperty(ref _universeIndex, value.KeepInRange(0, short.MaxValue)); }
        }

        private int _size = 512;
        /// <summary>
        /// The size of universe required. This is not (de)persisted, but written by client code after deserialization.
        /// </summary>
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }
    }
}
