
using System.Runtime.Serialization;

namespace GameshowPro.Common.Model;

/// <summary>
/// The best of lists and sets: order is maintained, while uniqueness is maintained and <see cref="Contains"/> is fast.
/// </summary>
/// <typeparam name="T">The type of elements in the list</typeparam>
public class OrderedSet<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISet<T>, IReadOnlySet<T>, IDeserializationCallback, IReadOnlyList<T>, ICollection, IList
{
    private readonly List<T> _items = [];
    private readonly HashSet<T> _itemsSet = [];

    public T this[int index] { get => _items[index]; set => _items[index] = value; }

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public bool IsSynchronized => ((ICollection)_items).IsSynchronized;

    public object SyncRoot => ((ICollection)_items).SyncRoot;

    public bool IsFixedSize => ((IList)_items).IsFixedSize;

    object? IList.this[int index] { get => ((IList)_items)[index]; set => ((IList)_items)[index] = value; }

    void ICollection<T>.Add(T item)
        => _ = Add(item);

    public bool Add(T item)
    {
        if (_itemsSet.Add(item))
        {
            _items.Add(item);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _items.Clear();
        _itemsSet.Clear();
    }

    public bool Contains(T item) => _itemsSet.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public void ExceptWith(IEnumerable<T> other) => _itemsSet.ExceptWith(other);

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    public int IndexOf(T item) => _items.IndexOf(item);

    public void Insert(int index, T item) => _items.Insert(index, item);

    public void IntersectWith(IEnumerable<T> other) => _itemsSet.IntersectWith(other);

    public bool IsProperSubsetOf(IEnumerable<T> other) => _itemsSet.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _itemsSet.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _itemsSet.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _itemsSet.Overlaps(other);

    public bool Remove(T item)
    {
        if (_itemsSet.Remove(item))
        {
            _items.Remove(item);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        T item = _items[index];
        _items.RemoveAt(index);
        _itemsSet.Remove(item);
    }

    public bool SetEquals(IEnumerable<T> other) => _itemsSet.SetEquals(other);

    public void SymmetricExceptWith(IEnumerable<T> other) => _itemsSet.SymmetricExceptWith(other);

    public void UnionWith(IEnumerable<T> other) => _itemsSet.UnionWith(other);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void OnDeserialization(object? sender) => _itemsSet.OnDeserialization(sender);

    public void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

    public int Add(object? value) => ((IList)_items).Add(value);

    public bool Contains(object? value) =>  ((IList)_items).Contains(value);

    public int IndexOf(object? value) => ((IList)_items).IndexOf(value);

    public void Insert(int index, object? value) => ((IList)_items).Insert(index, value);

    public void Remove(object? value) => ((IList)_items).Remove(value);
}
