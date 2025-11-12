
using System.Runtime.Serialization;

namespace GameshowPro.Common.Model;

/// <summary>
/// The best of lists and sets: order is maintained, while uniqueness is maintained and <see cref="Contains(T)"/> is fast.
/// </summary>
/// <typeparam name="T">The type of elements in the list</typeparam>
public class OrderedSet<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISet<T>, IReadOnlySet<T>, IDeserializationCallback, IReadOnlyList<T>, ICollection, IList
{
    private readonly List<T> _items = [];
    private readonly HashSet<T> _itemsSet = [];

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public T this[int index] { get => _items[index]; set => _items[index] = value; }

    /// <summary>Gets the number of elements contained in the set.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public int Count => _items.Count;

    /// <summary>Always false for this mutable collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool IsReadOnly => false;

    /// <summary>Gets a value indicating whether access to the collection is synchronized.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool IsSynchronized => ((ICollection)_items).IsSynchronized;

    /// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public object SyncRoot => ((ICollection)_items).SyncRoot;

    /// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool IsFixedSize => ((IList)_items).IsFixedSize;

    object? IList.this[int index] { get => ((IList)_items)[index]; set => ((IList)_items)[index] = value; }

    void ICollection<T>.Add(T item)
        => _ = Add(item);

    /// <summary>
    /// Adds an item to the set if it is not already present.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True if the item was added; false if it was already present.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public bool Add(T item)
    {
        if (_itemsSet.Add(item))
        {
            _items.Add(item);
            return true;
        }
        return false;
    }

    /// <summary>Removes all items from the set.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void Clear()
    {
        _items.Clear();
        _itemsSet.Clear();
    }

    /// <summary>Determines whether the set contains a specific value.</summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>True if found; otherwise false.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public bool Contains(T item) => _itemsSet.Contains(item);

    /// <summary>Copies the elements of the set to an array, starting at the specified index.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <summary>Removes all elements in the specified collection from the current set.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void ExceptWith(IEnumerable<T> other) => _itemsSet.ExceptWith(other);

    /// <summary>Returns an enumerator that iterates through the set.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    /// <summary>Returns the index of the specified item, or -1 if not found.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public int IndexOf(T item) => _items.IndexOf(item);

    /// <summary>
    /// Inserts an item at the specified index (does not enforce uniqueness).
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public void Insert(int index, T item) => _items.Insert(index, item);

    /// <summary>Modifies the current set so that it contains only elements that are also in a specified collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void IntersectWith(IEnumerable<T> other) => _itemsSet.IntersectWith(other);

    /// <summary>Determines whether the current set is a proper subset of a specified collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool IsProperSubsetOf(IEnumerable<T> other) => _itemsSet.IsProperSubsetOf(other);

    /// <summary>Determines whether the current set is a proper superset of a specified collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool IsProperSupersetOf(IEnumerable<T> other) => _itemsSet.IsProperSupersetOf(other);

    /// <summary>Determines whether the current set is a subset of a specified collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool IsSubsetOf(IEnumerable<T> other) => _itemsSet.IsSubsetOf(other);

    /// <summary>Determines whether the current set is a superset of a specified collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool IsSupersetOf(IEnumerable<T> other) => IsSupersetOf(other);

    /// <summary>Determines whether the current set overlaps with a specified collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool Overlaps(IEnumerable<T> other) => _itemsSet.Overlaps(other);

    /// <summary>Removes the specified item from the set.</summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was removed; otherwise false.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public bool Remove(T item)
    {
        if (_itemsSet.Remove(item))
        {
            _items.Remove(item);
            return true;
        }
        return false;
    }

    /// <summary>Removes the element at the specified index.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void RemoveAt(int index)
    {
        T item = _items[index];
        _items.RemoveAt(index);
        _itemsSet.Remove(item);
    }

    /// <summary>Determines whether the current set and the specified collection contain the same elements.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool SetEquals(IEnumerable<T> other) => _itemsSet.SetEquals(other);

    /// <summary>Modifies the current set so that it contains only elements that are present either in the set or in the specified collection, but not both.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void SymmetricExceptWith(IEnumerable<T> other) => _itemsSet.SymmetricExceptWith(other);

    /// <summary>Adds all elements in the specified collection to the current set.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void UnionWith(IEnumerable<T> other) => _itemsSet.UnionWith(other);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Runs when deserialization is complete.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void OnDeserialization(object? sender) => _itemsSet.OnDeserialization(sender);

    /// <summary>Copies the elements of the collection to an array, starting at a particular index.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

    /// <summary>Adds an item to the collection (non-generic interface).</summary>
    /// <remarks>Docs added by AI.</remarks>
    public int Add(object? value) => ((IList)_items).Add(value);

    /// <summary>Determines whether the collection contains a specific value (non-generic interface).</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool Contains(object? value) =>  ((IList)_items).Contains(value);

    /// <summary>Determines the index of a specific item (non-generic interface).</summary>
    /// <remarks>Docs added by AI.</remarks>
    public int IndexOf(object? value) => ((IList)_items).IndexOf(value);

    /// <summary>Inserts an item to the collection at the specified index (non-generic interface).</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void Insert(int index, object? value) => ((IList)_items).Insert(index, value);

    /// <summary>Removes the first occurrence of a specific object (non-generic interface).</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void Remove(object? value) => ((IList)_items).Remove(value);
}
