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
        internal byte[] Data;
        private Action<byte[]> _sendUpdate;
        public Action<byte[]> SendUpdate
        {
            get
            {
                return _sendUpdate;
            }
            set
            {
                _sendUpdate = value;
                _sendUpdate(Data);
            }
        }
        public Universe(UniverseSettings settings, int universeSize, Action<byte[]> sendUpdate)
        {
            _settings = settings;
            Data = new byte[universeSize + 1]; //First byte is DMX start code
            _sendUpdate = sendUpdate;
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
                        _sendUpdate?.Invoke(Data);
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
            if ((sourceData.Length + startOneBased) > Data.Length || startOneBased < 0)
            {
                throw new ArgumentOutOfRangeException("Source data goes outside of universe.");
            }
            var oldData = new byte[sourceData.Length];
            //Take copy of old values
            Array.Copy(Data, startOneBased, oldData, 0, oldData.Length);
            //Apply new values
            Array.Copy(sourceData, 0, Data, startOneBased, sourceData.Length);
            if (_sendUpdates)
            {
                _sendUpdate?.Invoke(Data);
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
            if (!channelOneBased.IsInRange(0, Data.Length, false))
            {
                throw new ArgumentOutOfRangeException(nameof(channel), channel, "Channel is outside universe");
            }
            if (Data[channelOneBased] != level)
            {
                Data[channelOneBased] = level;
                if (_sendUpdates)
                {
                    _sendUpdate?.Invoke(Data);
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
            Buffer.BlockCopy(Data, start, newArray, 0, length);
            return newArray;
        }
    }
}
