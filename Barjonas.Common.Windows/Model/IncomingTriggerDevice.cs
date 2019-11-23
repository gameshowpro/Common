// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Barjonas.Common.Model
{
    /// <summary>
    /// A base implementation of a trigger device which includes a list of triggers, but can be subclassed to add device-specific logic.
    /// </summary>
    public class IncomingTriggerDevice : NotifyingClass, IRemoteService
    {
        private IReadOnlyList<IncomingTrigger> _triggers;
        public IReadOnlyList<IncomingTrigger> Triggers
        {
            get
            {
                return _triggers;
            }
            protected set
            {
                if (_triggers != null)
                {
                    throw new InvalidOperationException("Triggers can only be set once");
                }

                _triggers = value;
                UpdateTriggerDict();
                if (_triggers != null)
                {
                    foreach (IncomingTrigger trigger in _triggers)
                    {
                        trigger.Setting.PropertyChanged += (s, e) => UpdateTriggerDict();
                    }
                }
            }
        }
        protected ReadOnlyDictionary<int, List<IncomingTrigger>> _triggerDict;
        public string Name { get; protected set; }

        public virtual string ServiceName
        {
            get
            {
                return Name;
            }
        }

        private RemoteServiceStates _serviceState;
        public RemoteServiceStates ServiceState
        {
            get { return _serviceState; }
            protected set { SetProperty(ref _serviceState, value); }
        }

        private string _stateDetail;
        public string StateDetail
        {
            get { return _stateDetail; }
            protected set { SetProperty(ref _stateDetail, value); }
        }

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

        public double Progress => 1d;

        private void UpdateTriggerDict()
        {
            var newTriggerDict = new Dictionary<int, List<IncomingTrigger>>();
            if (Triggers != null)
            {
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
            }
            _triggerDict = new ReadOnlyDictionary<int, List<IncomingTrigger>>(newTriggerDict);
            AfterUpdateTriggerDict();
        }

        protected virtual void AfterUpdateTriggerDict() { }
    }
}
