// (C) Barjonas LLC 2022


namespace GameshowPro.Common.Model;

[JsonObject(MemberSerialization.OptIn)]
public class ObservableClass : INotifyPropertyChanged
{
    protected bool _suppressEvents;
    protected bool _isDirty;
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual bool CompareEnumerablesByContent { get => false; }

    protected virtual void NotifyPropertyChanged(string name)
        => NotifyPropertyChanged(new PropertyChangedEventArgs(name));

    protected virtual void NotifyPropertyChanged(PropertyChangedEventArgs args)
        => PropertyChanged?.Invoke(this, args);

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
    protected virtual bool SetProperty<TField>(
        ref TField field, TField value, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", bool forceEvent = false)
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
                field = value;
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
            NotifyPropertyChanged(memberName);
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

