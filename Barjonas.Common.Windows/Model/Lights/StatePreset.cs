// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Newtonsoft.Json;

namespace Barjonas.Common.Model.Lights
{
    /// <summary>
    /// Represents a group of levels which go together to represent a particular state of a light.
    /// </summary>
    public class StateLevels : NotifyingClass
    {
        internal event EventHandler<IReadOnlyList<StatePresetChannel>> Flash;

        public StateLevels()
        {
            _flashTimer = new Timer((o) => DoFlash());
        }

        private string _key;
        [JsonProperty]
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                if (_key != null)
                {
                    throw new InvalidOperationException($"Once {nameof(Key)} is set, it is immutable.");
                }
                _key = value;
            }
        }

        private IReadOnlyList<StatePresetChannel> _levels;
        [JsonProperty]
        public IReadOnlyList<StatePresetChannel> Levels
        {
            get
            {
                return _levels;
            }
            set
            {
                _levels = value;
            }
        }

        private IReadOnlyList<StatePresetChannel> _flashLevels;
        public IReadOnlyList<StatePresetChannel> FlashLevels
        {
            get
            {
                return _flashLevels;
            }
            set
            {
                if (_flashLevels != null)
                {
                    throw new InvalidOperationException($"Once {nameof(FlashLevels)} is set, it is immutable.");
                }
                _flashLevels = value;
            }
        }

        private float _flashOnDuration = 0;
        [JsonProperty, DefaultValue(0)]
        public float FlashOnDuration
        {
            get { return _flashOnDuration; }
            set { SetProperty(ref _flashOnDuration, value); }
        }

        private float _flashOffDuration = 0;
        [JsonProperty, DefaultValue(0)]
        public float FlashOffDuration
        {
            get { return _flashOffDuration; }
            set { SetProperty(ref _flashOffDuration, value); }
        }

        private int _flashCount = 0;
        [JsonProperty, DefaultValue(0)]
        public int FlashCount
        {
            get { return _flashCount; }
            set { SetProperty(ref _flashCount, value); }
        }

        private readonly Timer _flashTimer;

        private void DoFlash()
        {
            _flashIsOn = !_flashIsOn;
            if (_flashIsOn)
            {
                _flashCounter++;
            }
            Flash?.Invoke(this, _flashIsOn ? Levels : FlashLevels);
            if (_flashCount <= 0 || _flashCounter < _flashCount)
            {
                _flashTimer.Change(TimeSpan.FromSeconds(_flashIsOn ? _flashOnDuration : _flashOffDuration), Timeout.InfiniteTimeSpan);
            }
        }

        private bool _flashIsOn;
        private int _flashCounter;
        public void ResetFlash(bool enable)
        {
            if (enable)
            {
                _flashCounter = 0;
                _flashIsOn = false;
                DoFlash();
            }
            else
            {
                _flashTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}
