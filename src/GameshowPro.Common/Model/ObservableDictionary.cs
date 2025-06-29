﻿namespace GameshowPro.Common.Model;

public class ObservableDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary, INotifyCollectionChanged, INotifyPropertyChanged
    where TKey : notnull
    where TValue : notnull
{
    private readonly IDictionary<TKey, TValue> _dictionary;

    /// <summary>Event raised when the collection changes.</summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged = (sender, args) => { };

    /// <summary>Event raised when a property on the collection changes.</summary>
    public event PropertyChangedEventHandler? PropertyChanged = (sender, args) => { };

    /// <summary>
    /// Occurs when a property is changed within an item.
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs>? ItemPropertyChanged;

    /// <summary>
    /// Initializes an instance of the class.
    /// </summary>
    public ObservableDictionary()
        : this(new Dictionary<TKey, TValue>())
    {
    }

    /// <summary>
    /// Initializes an instance of the class using another dictionary as 
    /// the key/value store.
    /// </summary>
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
        foreach (TValue value in _dictionary.Values)
        {
            AttachItemChangeHandler(value);
        }
    }

    private void AttachItemChangeHandler(TValue item)
    {
        if (item is INotifyPropertyChanged pc)
        {
            pc.PropertyChanged += BubbleItemPropertyChanged;
        }
    }

    private void DetatchItemChangeHandler(TValue item)
    {
        if (item is INotifyPropertyChanged pc)
        {
            pc.PropertyChanged -= BubbleItemPropertyChanged;
        }
    }

    private void BubbleItemPropertyChanged(object? sender, PropertyChangedEventArgs args)
        => ItemPropertyChanged?.Invoke(sender, args);

    private void AddWithNotification(KeyValuePair<TKey, TValue> item) => AddWithNotification(item.Key, item.Value);

    private void AddWithNotification(TKey key, TValue? value)
    {
        if (value is not null)
        {
            bool added = false;
            lock (_dictionary) //rudimentary thread safety
            {
                if (!_dictionary.ContainsKey(key))
                {
                    _dictionary.Add(key, value);
                    added = true;
                }
            }
            if (added)
            {
                AttachItemChangeHandler(value);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    new KeyValuePair<TKey, TValue>(key, value)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
            }
        }
    }

    private bool RemoveWithNotification(TKey key)
    {
        TValue? value;
        lock (_dictionary)
        {
            if (_dictionary.TryGetValue(key, out value))
            {
                _dictionary.Remove(key);
            }
            else
            {
                value = default;
            }
        }
        if (value is not null && !value.Equals(default))
        {
            DetatchItemChangeHandler(value);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                new KeyValuePair<TKey, TValue>(key, value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));

            return true;
        }

        return false;
    }

    private void UpdateWithNotification(TKey key, TValue? value)
    {
        if (_dictionary.TryGetValue(key, out TValue? existing))
        {
            DetatchItemChangeHandler(existing);
            if (value is not null)
            {
                lock (_dictionary)
                {
                    _dictionary[key] = value;
                }
                AttachItemChangeHandler(value);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, value),
                    new KeyValuePair<TKey, TValue>(key, existing)));
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
        }
        else
        {
            AddWithNotification(key, value);
        }
    }

    /// <summary>
    /// Allows derived classes to raise custom property changed events.
    /// </summary>
    protected void RaisePropertyChanged(PropertyChangedEventArgs args) => PropertyChanged?.Invoke(this, args);

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </summary>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    public void Add(TKey key, TValue value) => AddWithNotification(key, value);

    /// <summary>
    /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
    /// <returns>
    /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
    /// </returns>
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <summary>
    /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </summary>
    /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
    public ICollection<TKey> Keys => _dictionary.Keys;

    /// <summary>
    /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </returns>
    public bool Remove(TKey key) => RemoveWithNotification(key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
    /// <returns>
    /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
    /// </returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

    /// <summary>
    /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </summary>
    /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
    public ICollection<TValue> Values => _dictionary.Values;

    /// <summary>
    /// Gets or sets the element with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => UpdateWithNotification(key, value);
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => AddWithNotification(item);

    void ICollection<KeyValuePair<TKey, TValue>>.Clear() => Clear();

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

    int ICollection<KeyValuePair<TKey, TValue>>.Count => _dictionary.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => _dictionary.IsReadOnly;

    ICollection IDictionary.Keys => (ICollection)_dictionary.Keys;

    ICollection IDictionary.Values => (ICollection)_dictionary.Values;

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public int Count => _dictionary.Count;

    public object SyncRoot => ((ICollection)_dictionary).SyncRoot;

    public bool IsSynchronized => ((ICollection)_dictionary).IsSynchronized;

    public object? this[object key]
    {
        get => _dictionary[(TKey)key];
        set => UpdateWithNotification((TKey)key, (TValue?)value);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => RemoveWithNotification(item.Key);

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    public bool Contains(object key) => ContainsKey((TKey)key);

    public void Add(object key, object? value) => AddWithNotification((TKey)key, (TValue?)value);

    public void Clear()
    {
        lock (_dictionary)
        {
            foreach (TValue value in _dictionary.Values)
            {
                DetatchItemChangeHandler(value);
            }
            _dictionary.Clear();
        }

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
    }

    public IDictionaryEnumerator GetEnumerator() => ((IDictionary)_dictionary).GetEnumerator();

    public void Remove(object key) => RemoveWithNotification((TKey)key);

    public void CopyTo(Array array, int index)
    {
        if (array is KeyValuePair<TKey, TValue>[] pairs)
        {
            CopyTo(pairs, index);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
