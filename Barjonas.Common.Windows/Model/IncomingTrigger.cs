// (C) Barjonas LLC 2018

using System;
using System.Diagnostics;
using Barjonas.Common.ViewModel;
using NLog;
#nullable enable
namespace Barjonas.Common.Model
{
    public abstract class IncomingTrigger : NotifyingClass, ITrigger
    {
        private readonly Stopwatch _lastTrigger = new();
        protected Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public delegate void IsDownChangedEventHandler(IncomingTrigger sender, bool isDown);
        /// <summary>
        /// Event raised after PropertyChanged for change to IsDown state, saving the consumer the overhead of checking the property name and value.
        /// </summary>
        public event IsDownChangedEventHandler? IsDownChanged;

        public event EventHandler<TriggerArgs>? Triggered;

        protected IncomingTrigger(IncomingTriggerSetting setting)
        {
            _lastTrigger.Start();
            Setting = setting;
            SimulateTriggerCommand = new RelayCommand<bool?>((latch) => { if (latch == true) { IsDown = !_isDown; } else { DoTriggered(); } });
            ToggleIsEnabledCommand = new(() => { Setting.IsEnabled = !Setting.IsEnabled; });
        }

        protected void OnIsDownChanged(bool value)
            => IsDownChanged?.Invoke(this, value);

        public IncomingTriggerSetting Setting { get; }

        protected bool _isDown;
        public virtual bool IsDown
        {
            get { return _isDown; }
            protected set
            {
                if (SetProperty(ref _isDown, value))
                {
                    OnIsDownChanged(value);
                    if (value && Setting.IsEnabled && TriggerWhenDown)
                    {
                        DoTriggered();
                    }
                }
            }
        }

        public object? TriggerData { get; set; }

        /// <summary>
        /// Called by base class whenever this trigger has been triggered, subject to the currently configured conditions.
        /// The base implementation raises the Triggered event, so should be called from subclasses.
        /// </summary>
        protected virtual void DoTriggered()
        {
            if (!Setting.DebounceInterval.HasValue || _lastTrigger.Elapsed > Setting.DebounceInterval.Value)
            {
                _lastTrigger.Restart();
                LastTriggerDateTime = DateTime.UtcNow;
                Triggered?.Invoke(this, new TriggerArgs(TriggerData));
            }
        }

        /// <summary>
        /// If true, <see cref="Triggered"/> will only be called whenever <see cref="IsDown"/> changes to true.
        /// Previously known as IsEnabled.
        /// </summary>
        public virtual bool TriggerWhenDown => true;

        private int? _ordinal;
        /// <summary>
        /// The ordinal which was specified by the most recent triggerer.
        /// </summary>
        public int? Ordinal
        {
            get { return _ordinal; }
            set { SetProperty(ref _ordinal, value); }
        }

        private TimeSpan? _time;
        /// <summary>
        /// The time which was specified by the most recent triggerer.
        /// </summary>
        public TimeSpan? Time
        {
            get { return _time; }
            set { SetProperty(ref _time, value); }
        }

        private bool _isTest;
        /// <summary>
        /// The IsTest decaration which was specified by the most recent triggerer.  Primarily intended for to help log clarification.
        /// </summary>
        public bool IsTest
        {
            get { return _isTest; }
            set { SetProperty(ref _isTest, value); }
        }

        private DateTime _lastTriggerDateTime = DateTime.MinValue;
        public DateTime LastTriggerDateTime
        {
            get { return _lastTriggerDateTime; }
            protected set { SetProperty(ref _lastTriggerDateTime, value); }
        }

        public RelayCommand<bool?> SimulateTriggerCommand { get; private set; }
        public RelayCommandSimple ToggleIsEnabledCommand { get; private set; }
    }
}
#nullable restore
