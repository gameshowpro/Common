// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
#nullable enable
namespace Barjonas.Common.Model
{
    /// <summary>
    /// A base implementation of a trigger device which includes a list of triggers, but can be subclassed to add device-specific logic.
    /// </summary>
    public abstract class IncomingTriggerDevice : NotifyingClass, IRemoteService
    {
        public IncomingTriggerDevice(string name, IEnumerable<IncomingTrigger>? triggers, ServiceState serviceState)
        {
            Name = name;
            Triggers = triggers?.ToImmutableList() ?? ImmutableList<IncomingTrigger>.Empty;
            UpdateTriggerDict();
            foreach (IncomingTrigger trigger in Triggers)
            {
                trigger.Setting.PropertyChanged += (s, e) => UpdateTriggerDict();
            }
            ServiceState = serviceState;
        }
        public ImmutableList<IncomingTrigger> Triggers { get; }
        protected ImmutableDictionary<int, ImmutableList<IncomingTrigger>> _triggerDict;
        public string Name { get; }

        public ServiceState ServiceState { get; }

        private bool _allowDuplicateTriggerIds = false;
        /// <summary>
        /// By default, triggers must have unique IDs. If this property is true, multiple triggers may share an ID,
        /// so that they will all be triggered when a remote trigger is received with that ID.
        /// </summary>
        public bool AllowDuplicateTriggerIds
        {
            get { return _allowDuplicateTriggerIds; }
            set { SetProperty(ref _allowDuplicateTriggerIds, value); }
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            protected set => _ = SetProperty(ref _progress, value);
        }

        [MemberNotNull(nameof(_triggerDict))]
        private void UpdateTriggerDict()
        {
            Dictionary<int, List<IncomingTrigger>> newTriggerDict = new();

            foreach (IncomingTrigger trigger in Triggers)
            {
                if (newTriggerDict.ContainsKey(trigger.Setting.Id))
                {
                    trigger.Setting.IdIsValid = _allowDuplicateTriggerIds;
                    if (_allowDuplicateTriggerIds)
                    {
                        newTriggerDict[trigger.Setting.Id].Add(trigger);
                    }
                }
                else
                {
                    newTriggerDict.Add(trigger.Setting.Id, new List<IncomingTrigger>() { trigger });
                    trigger.Setting.IdIsValid = true;
                }
            }
            _triggerDict = newTriggerDict.ToImmutableDictionary((pair) => pair.Key, (pair) => pair.Value.ToImmutableList());
            AfterUpdateTriggerDict();
        }

        protected virtual void AfterUpdateTriggerDict() { }
    }
}
#nullable restore
