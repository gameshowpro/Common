// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Barjonas.Common.Model
{
    public struct PropertyChangeCondition
    {
        internal readonly INotifyPropertyChanged _sender;
        internal readonly INotifyCollectionChanged _senderCollection;
        internal readonly string _property;

        public PropertyChangeCondition(INotifyPropertyChanged sender, string property)
        {
            _sender = sender;
            _property = property;
            _senderCollection = null;
        }

        public PropertyChangeCondition(INotifyCollectionChanged sender)
        {
            _sender = null;
            _property = null;
            _senderCollection = sender;
        }

        public PropertyChangeCondition(INotifyCollectionChanged sender, string property)
        {
            _sender = null;
            _property = property;
            _senderCollection = sender;
        }
    }

    /// <summary>
    /// This will execute a given delegate whenever one specified properties changes on one of the specified object.
    /// </summary>
    public class PropertyChangeFilter
    {
        private readonly PropertyChangedEventHandler _handler;
        private readonly HashSet<PropertyChangeCondition> _conditions;
        internal readonly bool _isValid;
        internal PropertyChangeFilter(PropertyChangedEventHandler action, IEnumerable<PropertyChangeCondition> conditions)
        {
            IEnumerable<PropertyChangeCondition> validCons = conditions.Where(c => c._sender != null || c._senderCollection != null);
            if (!validCons.Any())
            {
                return;
            }
            _isValid = true;
            _handler = action;
            _conditions = new HashSet<PropertyChangeCondition>();
            var senders = new HashSet<INotifyPropertyChanged>();
            var collectionSenders = new HashSet<INotifyCollectionChanged>();
            foreach (PropertyChangeCondition condition in validCons)
            {
                //ensure only one even registration per sender, even if we are monitoring multiple properties
                if (condition._sender == null)
                {
                    if (!collectionSenders.Contains(condition._senderCollection))
                    {
                        collectionSenders.Add(condition._senderCollection);
                        condition._senderCollection.CollectionChanged += SenderCollection_CollectionChanged;
                        if (condition._senderCollection is IItemPropertyChanged ipc)
                        {
                            ipc.ItemPropertyChanged += Ipc_ItemPropertyChanged;
                        }
                    }
                }
                else
                {
                    if (!senders.Contains(condition._sender))
                    {
                        senders.Add(condition._sender);
                        condition._sender.PropertyChanged += Sender_PropertyChanged;
                    }
                }
                _conditions.Add(condition);
            }
            _handler?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        private void Ipc_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            var condition = new PropertyChangeCondition(sender as INotifyCollectionChanged, e.PropertyName);
            if (_conditions.Contains(condition))
            {
                _handler?.Invoke(sender, new PropertyChangedEventArgs(e.PropertyName.ToString()));
            }
        }

        private void SenderCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var condition = new PropertyChangeCondition(sender as INotifyCollectionChanged);
            if (_conditions.Contains(condition))
            {
                _handler?.Invoke(sender, new PropertyChangedEventArgs(e.Action.ToString()));
            }
        }

        private void Sender_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var condition = new PropertyChangeCondition(sender as INotifyPropertyChanged, e.PropertyName);
            if (_conditions.Contains(condition))
            {
                _handler?.Invoke(sender, e);
            }
        }

        internal void Release()
        {
            foreach (PropertyChangeCondition condition in _conditions ?? Enumerable.Empty<PropertyChangeCondition>())
            {
                if (condition._sender == null)
                {
                    condition._senderCollection.CollectionChanged -= SenderCollection_CollectionChanged;
                }
                else
                {
                    condition._sender.PropertyChanged -= Sender_PropertyChanged;
                }
            }
        }
    }

    public class PropertyChangeFilters
    {
        private readonly List<PropertyChangeFilter> _filters = new List<PropertyChangeFilter>();
        public void AddFilter(PropertyChangedEventHandler handler, params PropertyChangeCondition[] conditions)
        {
            AddFilter(handler, (IEnumerable<PropertyChangeCondition>)conditions);
        }

        public void AddFilter(PropertyChangedEventHandler handler, IEnumerable<PropertyChangeCondition> conditions)
        {
            var filter = new PropertyChangeFilter(handler, conditions);
            if (filter._isValid)
            {
                _filters.Add(filter);
            }

        }

        public void ClearFilters()
        {
            foreach (PropertyChangeFilter f in _filters)
            {
                f.Release();
            }
            _filters.Clear();
        }

        public bool Any() => _filters.Any();
    }
}
