// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Barjonas.Common.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NotifyingClass : INotifyPropertyChanged
    {
        protected static Dispatcher s_dispatcher = Dispatcher.CurrentDispatcher;
        private bool _supressEvents = false;
        private bool _isDirty;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            if (s_dispatcher.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            else
            {
                if (PropertyChanged != null)
                {
                    s_dispatcher.BeginInvoke(PropertyChanged, this, new PropertyChangedEventArgs(name));
                }
            }
        }
        public bool SupressEvents
        {
            get
            { return _supressEvents; }
            set
            {
                if (_supressEvents != value)
                {
                    _supressEvents = value;
                    if (_supressEvents && _isDirty)
                    {
                        //Something changed while events were supressed
                        PropertyChanged(this, new PropertyChangedEventArgs(""));
                        _isDirty = false;
                    }
                }
            }
        }

        /// <summary>
        /// Set a property, if it has changed, and raise event as appropriate.  Return boolean indicated whether any change was made.
        /// </summary>
        protected bool SetProperty<F>(ref F field, F value, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (((field == null) != (value == null)) || ((field != null) && !field.Equals(value)))
            {
                if (string.IsNullOrWhiteSpace(memberName))
                {
                    throw new ArgumentException($"{memberName} cannot be null, empty or whitespace");
                }

                field = value;
                if (_supressEvents)
                {
                    _isDirty = true;
                }
                else
                {
                    NotifyPropertyChanged(memberName);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set a property which is backed by an item from an array. If it has changed, and throw event as appropriate.  Return boolean indicated whether any change was made.
        /// </summary>
        protected bool SetProperty<F>(IList<F> fieldList, int fieldIndex, F value, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            F field = fieldList[fieldIndex];
            if (SetProperty(ref field, value, memberName))
            {
                fieldList[fieldIndex] = field;
                return true;
            }
            return false;
        }
    }
}
