// (C) Barjonas LLC 2022

namespace GameshowPro.Common.Model;

/// <summary>
/// Base class that implements property change notification with optional UI-thread dispatching.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class ObservableClass : INotifyPropertyChanged
{
#if WPF
    protected readonly Dispatcher _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
    protected readonly Action<PropertyChangedEventArgs> NotifyPropertyChangedAction;

    public ObservableClass()
    {
        NotifyPropertyChangedAction = new(NotifyPropertyChanged);
    }
#endif


    /// <summary>
    /// A subclass of PropertyChangedEventArgs that <see cref="PropertyChangedOnOriginalThread"/> has already been raised and should not be raised again.
    /// Only created and consumed within this <see cref="ObservableClass"/>.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    private class PropertyChangeEventArgsAlreadyRaisedOnOriginalThread(string propertyName) : PropertyChangedEventArgs(propertyName)
    {
    }
    [IgnoreDataMember]
    protected bool _suppressEvents;
    [IgnoreDataMember]
    protected bool _isDirty;
    /// <summary>
    /// Raised when property on the subclass is changed.  This event is intended for UI binding and is always dispatched onto the UI thread.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Raised when property on the subclass is changed.  This event is always raised on the thread that set the property, so should not be used for UI binding.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChangedOnOriginalThread;
    /// <summary>
    /// When true, enumerable properties are compared by their contents instead of reference equality when determining change.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    [IgnoreDataMember]
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
#if WPF
        if (_dispatcher?.CheckAccess() != false)
        {
            PropertyChanged?.Invoke(this, args);
        }
        else
        {
            _dispatcher.BeginInvoke(NotifyPropertyChangedAction, args);
        }
#else
        PropertyChanged?.Invoke(this, args);
#endif

    }

    /// <summary>
    /// When set, suppresses event emission; a single empty change will be raised when re-enabled if changes occurred.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    [IgnoreDataMember]
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
                    PropertyChangedOnOriginalThread?.Invoke(this, new PropertyChangedEventArgs(""));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                    _isDirty = false;
                }
            }
        }
    }

    /// <summary>
    /// Set a property, if it has changed, and raise event as appropriate.  Return boolean indicated whether any change was made.
    /// </summary>
    /// <param name="field">The backing field to update when the new value differs.</param>
    /// <param name="value">The proposed new value.</param>
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
    /// Sets a property value that is stored in a list at a specific index, raising change events if the value changes.
    /// </summary>
    /// <typeparam name="F">The element type.</typeparam>
    /// <param name="fieldList">The list that backs the property.</param>
    /// <param name="fieldIndex">The index of the element in the list.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="memberName">Caller-provided member name; defaults automatically.</param>
    /// <returns>True if the value changed; otherwise false.</returns>
    /// <remarks>Docs added by AI.</remarks>
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

    /// <summary>
    /// Determines whether two values are different, with optional enumerable content comparison.
    /// </summary>
    /// <typeparam name="F">The value type.</typeparam>
    /// <param name="compareEnumerablesByContent">When true, compares enumerable contents element-by-element.</param>
    /// <param name="field">The current value.</param>
    /// <param name="value">The new value.</param>
    /// <returns>True if the values differ; otherwise false.</returns>
    /// <remarks>Docs added by AI.</remarks>
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

    /// <summary>
    /// Compares two non-generic enumerables for sequence equality, optimized for common collection types.
    /// </summary>
    /// <param name="first">The first sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <returns>True if sequences are equal; otherwise false.</returns>
    /// <remarks>Docs added by AI.</remarks>
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

