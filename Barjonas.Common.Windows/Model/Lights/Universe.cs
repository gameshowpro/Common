// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Linq;

namespace Barjonas.Common.Model.Lights
{
    /// <summary>
    /// Represents an abstract lighting universe, regardless of the transport layer
    /// </summary>
    public class Universe
    {
        internal byte[] _data;
        public event Action<byte[]>? SendUpdate;

        public Universe(UniverseSettings settings, int universeSize)
        {
            _settings = settings;
            _data = new byte[universeSize + 1]; //First byte is DMX start code
            _channels = new List<UniverseChannel>();
            for (var i = 1; i <= universeSize; i++)
            {
                _channels.Add(new UniverseChannel(this, i - 1));
            }
        }

        private readonly UniverseSettings _settings;
        public UniverseSettings Settings
        {
            get { return _settings; }
        }

        private readonly List<UniverseChannel> _channels;
        public List<UniverseChannel> Channels
        {
            get { return _channels; }
        }

        private bool _isDirty;

        private bool _sendUpdates = true;
        /// <summary>
        /// If true, updates will be sent immediately on change.  Otherwise state will be held internally until SendUpdates is set to true.
        /// In other words, this allows changes to be batched.
        /// </summary>
        public bool SendUpdates
        {
            get { return _sendUpdates; }
            set
            {
                if (_sendUpdates != value)
                {
                    _sendUpdates = value;
                    if (_sendUpdates & _isDirty)
                    {
                        _isDirty = false;
                        SendUpdate?.Invoke(_data);
                    }
                }
            }
        }

        /// <summary>
        /// Set a block of data in a single operation.
        /// </summary>
        /// <param name="sourceData">The data block containing the new values.</param>
        /// <param name="start">The zero-based channel number of the first byte.  Value of -1 will set DMX start code from first byte.</param>
        public void SetData(byte[] sourceData, int start)
        {
            var startOneBased = start + 1;  //must account for DMX start code which is at position zero
            if ((sourceData.Length + startOneBased) > _data.Length || startOneBased < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceData),"Source data goes outside of universe.");
            }
            var oldData = new byte[sourceData.Length];
            //Take copy of old values
            Array.Copy(_data, startOneBased, oldData, 0, oldData.Length);
            //Apply new values
            Array.Copy(sourceData, 0, _data, startOneBased, sourceData.Length);
            if (_sendUpdates)
            {
                SendUpdate?.Invoke(_data);
            }
            else
            {
                _isDirty = true;
            }
            //Find changes
            for (var i = 0; i < sourceData.Length; i++)
            {
                if (sourceData[i] != oldData[i])
                {
                    _channels[i].LevelChanged();
                }
            }
        }

        /// <summary>
        /// Set a single channel value by number (rather than on the channel object).
        /// </summary>
        /// <param name="channel">The zero-based channel number.</param>
        /// <param name="level">The zero-based level to set.</param>
        public void SetData(int channel, byte level)
        {
            var channelOneBased = channel + 1;
            if (!channelOneBased.IsInRange(0, _data.Length, false))
            {
                throw new ArgumentOutOfRangeException(nameof(channel), channel, "Channel is outside universe");
            }
            if (_data[channelOneBased] != level)
            {
                _data[channelOneBased] = level;
                if (_sendUpdates)
                {
                    SendUpdate?.Invoke(_data);
                }
                else
                {
                    _isDirty = true;
                }
                _channels[channel].LevelChanged();
            }
        }

        public byte[] GetData(int start, int length)
        {
            var newArray = new byte[length];
            Buffer.BlockCopy(_data, start, newArray, 0, length);
            return newArray;
        }
    }
}
