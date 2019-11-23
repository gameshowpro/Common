// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Barjonas.Common.Model.Lights
{
    /// <summary>
    /// Represents a single logical light fixture.  This will contain one or more channels, e.g. different colors.  Each State represents values a fixed set of values for these channels.
    /// </summary>
    public class Fixture : NotifyingClass
    {
        private StatePresetGroup _stateGroup;
        public StatePresetGroup StateGroup
        {
            get { return _stateGroup; }
            set
            {
                if (_stateGroup != null)
                {
                    throw new InvalidOperationException($"Once {nameof(StateGroup)} is set, it is immutable.");
                }
                _stateGroup = value ?? throw new ArgumentNullException($"{nameof(StateGroup)} cannot be set to null.");
                State = _stateGroup.StatesLevels.FirstOrDefault();
            }
        }

        private StateLevels _state;
        public StateLevels State
        {
            get { return _state; }
            set
            {
                if (_stateGroup == null)
                {
                    throw new InvalidOperationException($"Can't set a {nameof(State)} without a {nameof(StateGroup)} already set.");
                }

                if (value == null)
                {
                    throw new ArgumentNullException($"{nameof(State)} cannot be set to null.");
                }

                if (!_stateGroup.StatesLevels.Contains(value))
                {
                    throw new ArgumentException($"Can't set a {nameof(State)} which does not belong to configured {nameof(StateGroup)}.");
                }

                SetProperty(ref _state, value);
            }
        }

        private string _key;
        [JsonProperty, DefaultValue(null)]
        public string Key
        {
            get { return _key; }
            set
            {
                if (_key != null)
                {
                    throw new InvalidOperationException($"Once {nameof(Key)} is set, it is immutable.");
                }
                _key = value;
            }
        }

        private string _displayName;
        [JsonProperty, DefaultValue("Light")]
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private BindingList<FixtureChannel> _channels;
        [JsonProperty, DefaultValue(null)]
        public BindingList<FixtureChannel> Channels
        {
            get { return _channels; }
            set
            {
                RemoveChannelHandlers(_channels, Channel_PropertyChanged);
                SetProperty(ref _channels, value);
                AddChannelHandlers(_channels, Channel_PropertyChanged);
                UpadateMixedColor();
            }
        }

        public void ClearAll()
        {
            ApplyStatePreset((IEnumerable<StatePresetChannel>)null);
        }

        public void ReapplyStatePreset()
        {
            ApplyStatePreset(_currentState?.Levels);
        }

        public void ApplyStatePreset(string presetKey)
        {
            ApplyStatePreset(_stateGroup.StatesLevels[presetKey]);
        }

        public void ApplyStatePreset(IEnumerable<StatePresetChannel> preset)
        {
            var i = 0;
            foreach (FixtureChannel chan in _channels)
            {
                chan.Level = preset?.ElementAtOrDefault(i)?.Level ?? 0;
                i++;
            }
        }

        public void ApplyStatePreset(StateLevels stateLevels, bool flashNow = true)
        {
            if (_currentState != null)
            {
                _currentState.Flash -= CurrentState_Flash;
            }
            _currentState = stateLevels;
            if (_currentState != null)
            {
                _currentState.Flash += CurrentState_Flash;
            }
            if (stateLevels?.FlashOnDuration > 0 && stateLevels?.FlashOffDuration > 0)
            {
                if (flashNow)
                {
                    _currentState?.ResetFlash(true);
                }
            }
            else
            {
                ApplyStatePreset(stateLevels?.Levels);
            }
        }

        private void CurrentState_Flash(object sender, IReadOnlyList<StatePresetChannel> e)
        {
            ApplyStatePreset(e);
        }

        private StateLevels _currentState;

        private static void AddChannelHandlers(IList<FixtureChannel> channels, PropertyChangedEventHandler handler)
        {
            if (channels == null)
            { return; }
            foreach (FixtureChannel ch in channels)
            {
                ch.PropertyChanged += handler;
                ch.FixtureChannelType.PropertyChanged += handler;
            }
        }

        private static void RemoveChannelHandlers(IList<FixtureChannel> channels, PropertyChangedEventHandler handler)
        {
            if (channels == null)
            { return; }
            foreach (FixtureChannel ch in channels)
            {
                ch.PropertyChanged -= handler;
                ch.FixtureChannelType.PropertyChanged -= handler;
            }
        }

        private void Channel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpadateMixedColor();
        }

        private void UpadateMixedColor()
        {
            Color color = Colors.Black;
            foreach (FixtureChannel ch in _channels)
            {
                color += Color.Multiply(ch.FixtureChannelType.Primary, ch.Level / 255f);
            }
            MixedColor = color;
        }


        private Color _mixedColor;
        [DefaultValue("#00000000")]
        public Color MixedColor
        {
            get { return _mixedColor; }
            private set { SetProperty(ref _mixedColor, value); }
        }
    }
}
