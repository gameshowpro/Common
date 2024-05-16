// (C) Barjonas LLC 2022


namespace GameshowPro.Common.Model;

[JsonObject(MemberSerialization.OptIn)]
public class ObservableClass : INotifyPropertyChanged
{
    /// <summary>
    /// A subclass of PropertyChangedEventArgs that <see cref="PropertyChangedOnOriginalThread"/> has already been raised and should not be raised again.
    /// Only created and consumed within this <see cref="ObservableClass"/>.
    /// </summary>
    /// <param name="propertyName"></param>
    private class PropertyChangeEventArgsAlreadyRaisedOnOriginalThread(string propertyName) : PropertyChangedEventArgs(propertyName)
    {
    }

    protected bool _suppressEvents;
    protected bool _isDirty;
    /// <summary>
    /// Raised when property on the subclass is changed.  This event is intended for UI binding and is always dispatched onto the UI thread.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Raised when property on the subclass is changed.  This event is always raised on the thread that set the property, so should not be used for UI binding.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChangedOnOriginalThread;
    protected virtual bool CompareEnumerablesByContent { get => false; }

    /// <summary>
    /// Subclasses can override this method to insert their own logic before raising an event to subscribers.
    /// For example, dispatching the event to the UI thread.
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        => NotifyPropertyChanged(args);

    /// <summary>
    /// Allows subclasses to raise <see cref="PropertyChanged"/> events to subscribers by name.
    /// </summary>
    protected void NotifyPropertyChanged(string name)
        => NotifyPropertyChanged(new PropertyChangedEventArgs(name));

    /// <summary>
    /// Allows subclasses to raise <see cref="PropertyChanged"/> events to subscribers with a pre-created <see cref="PropertyChangedEventArgs"/>.
    /// </summary>
    protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
    {
        if (args is not PropertyChangeEventArgsAlreadyRaisedOnOriginalThread)
        {
            //This execution was not chained from OnPropertyChanged fired from SetProperty within this class, so we still haven't raised PropertyChangedOnOriginalThread.
            PropertyChangedOnOriginalThread?.Invoke(this, args);
        }
        PropertyChanged?.Invoke(this, args);
    }

    public bool SuppressEvents
    {
        get
        { return _suppressEvents; }
        set
        {
            if (_suppressEvents != value)
            {
                _suppressEvents = value;
                if (_suppressEvents && _isDirty)
                {
                    //Something changed while events were suppressed
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                    _isDirty = false;
                }
            }
        }
    }

    /// <summary>
    /// Set a property, if it has changed, and raise event as appropriate.  Return boolean indicated whether any change was made.
    /// </summary>
    /// <param name="memberName">The name of the member to set, to be used in event invocations. Automatically set to CallerMemberName.</param>
    /// <param name="forceEvent">If true, event is raised regardless of whether there was a change.</param>
    /// <param name="beforeChangeDelegate">If a pending change is detected, this delegate will be invoked before the change is applied. This is the last chance to access the old value. Note: the delegate is run on the calling thread rather than any dispatcher.</param>
    /// <param name="afterChangeDelegate">If a pending change is detected, this delegate will be invoked after the change is applied. This is an alternative a conditional statement based on the return value. Note: the delegate is run on the calling thread rather than any dispatcher.</param>
    protected virtual bool SetProperty<TField>(
        ref TField field, TField value, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", bool forceEvent = false, Action? beforeChangeDelegate = null, Action? afterChangeDelegate = null)
    {
        bool changed = false;
        if ((field == null != (value == null)) || (field != null))
        {
            if (string.IsNullOrWhiteSpace(memberName))
            {
                throw new ArgumentException($"{memberName} cannot be null, empty or whitespace");
            }
            if (HasChanged(CompareEnumerablesByContent, field, value))
            {
                changed = true;
                beforeChangeDelegate?.Invoke();
                field = value;
                afterChangeDelegate?.Invoke();
                if (_suppressEvents)
                {
                    _isDirty = true;
                }
            }
            else if (CompareEnumerablesByContent)
            {
                //Could be that enumerable changed, but contents didn't. Do the assignment, but don't raise an event.
                field = value;
            }
        }
        if (forceEvent || (changed && !_suppressEvents))
        {
            PropertyChangeEventArgsAlreadyRaisedOnOriginalThread args = new(memberName);
            PropertyChangedOnOriginalThread?.Invoke(this, args);
            OnPropertyChanged(args);
        }
        return changed;
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

    protected static bool HasChanged<F>(bool compareEnumerablesByContent, F field, F value)
    {
        if (field == null)
        {
            return value != null;
        }
        else if (value == null)
        {
            return true;
        }
        if (compareEnumerablesByContent && field is IEnumerable eField && value is IEnumerable eValue)
        {
            return !SequenceEqual(eField, eValue);
        }
        else
        {
            return !field.Equals(value);
        }
    }

    public static bool SequenceEqual(IEnumerable first, IEnumerable second)
    {
        if (first is ICollection firstCol && second is ICollection secondCol)
        {
            if (firstCol.Count != secondCol.Count)
            {
                return false;
            }

            if (firstCol is IList firstList && secondCol is IList secondList)
            {
                int count = firstCol.Count;
                for (int i = 0; i < count; i++)
                {
                    if (firstList[i]?.Equals(secondList[i]) != true)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        IEnumerator e1 = first.GetEnumerator();
        IEnumerator e2 = second.GetEnumerator();
        {
            while (e1.MoveNext())
            {
                if (!(e2.MoveNext() && e1.Current.Equals(e2.Current)))
                {
                    return false;
                }
            }

            return !e2.MoveNext();
        }
    }
}

