namespace Barjonas.Common.Model;

public abstract class ObservableKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>, INotifyCollectionChanged, INotifyPropertyChanged where TKey : IComparable
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    int _prevCount = 0;

    protected virtual bool EnforceOrderByKey => false;

    protected override void SetItem(int index, TItem item)
    {
        TItem oldItem = this.ElementAt(index);
        base.SetItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
    }

    protected override void InsertItem(int index, TItem item)
    {
        if (EnforceOrderByKey)
        {
            TKey key = GetKeyForItem(item);
            index = Items.TakeWhile(i => GetKeyForItem(i).CompareTo(key) < 0).Count();
        }
        base.InsertItem(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void RemoveItem(int index)
    {
        TItem item = this.ElementAt(index);
        base.RemoveItem(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
    }

    private bool _deferNotifyCollectionChanged = false;
    public void AddRange(IEnumerable<TItem> items)
    {
        try
        {
            _deferNotifyCollectionChanged = true;
            foreach (TItem item in items)
            {
                Add(item);
            }
        }
        finally { _deferNotifyCollectionChanged = false; }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_deferNotifyCollectionChanged)
        {
            return;
        }

        CollectionChanged?.Invoke(this, e);
        if (Count != _prevCount)
        {
            _prevCount = Count;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }

    protected virtual void InvokePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Method to add a new object to the collection, or to replace an existing one if there is 
    /// already an object with the same key in the collection.
    /// </summary>
    public void AddOrReplace(TItem item)
    {
        int i = GetItemIndex(item);
        if (i != -1)
        {
            base.SetItem(i, item);
        }
        else
        {
            base.Add(item);
        }
    }

    /// <summary>
    /// Method to replace an existing object in the collection, i.e., an object with the same key. 
    /// An exception is thrown if there is no existing object with the same key.
    /// </summary>
    public void Replace(TItem item)
    {
        int i = GetItemIndex(item);
        if (i != -1)
        {
            base.SetItem(i, item);
        }
        else
        {
            throw new Exception("Object to be replaced not found in collection.");
        }
    }

    /// <summary>
    /// Method to get the index into the List{} in the base collection for an item that may or may 
    /// not be in the collection. Returns -1 if not found.
    /// </summary>
    private int GetItemIndex(TItem item)
    {
        TKey keyToFind = GetKeyForItem(item);
        return BaseList?.FindIndex((TItem existingItem) =>
                                  GetKeyForItem(existingItem).Equals(keyToFind)) ?? -1;
    }

    /// <summary>
    /// Property to provide access to the "hidden" List{} in the base class.
    /// </summary>
    public List<TItem>? BaseList
    {
        get => Items as List<TItem>;
    }

    /// <summary>
    /// Remove all items in this collection which have keys that do not exist in the given ObservableKeyedCollection.
    /// </summary>
    /// <param name="source">The ObservableKeyedCollection containing all the keys which should remain in this instance.</param>
    public void RemoveWhereMissing(ObservableKeyedCollection<TKey, TItem> source)
    {
        if (source?.Dictionary == null)
        {
            Clear();
            return;
        }
        for (int i = Count - 1; i >= 0; i--)
        {
            if (!source.Dictionary.ContainsKey(GetKeyForItem(this.ElementAt(i))))
            {
                source.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// If the key exits, return the associated item, otherwise create one, add it and return that.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public TItem GetOrCreate(TKey key, Func<TKey, TItem> factory)
    {
        if (Contains(key))
        {
            return this[key];
        }
        else
        {
            TItem n = factory.Invoke(key);
            Add(n);
            return n;
        }
    }
}
