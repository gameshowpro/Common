
namespace GameshowPro.Common.Model;

public interface IItemPropertyChanged
{
    event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;
}
/// <summary>
/// Subclass of ObservableCollection which implements all all IObservableCollection events fully.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableCollectionEx<T> : ObservableCollection<T>, IItemPropertyChanged where T : INotifyPropertyChanged
{
    /// <summary>
    /// Raise once for each batch of changes, independently of INotifyCollectionChanged, which has many complex issues in implementation making it unrelible.
    /// </summary>
    public event EventHandler? BatchChange;
    private bool _batchChangeSuppressed = false;
    protected void OnBatchChange()
    {
        if (!_batchChangeSuppressed)
        {
            BatchChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public static ObservableCollectionEx<T> Empty { get; } = [];

    /// <summary>
    /// Occurs when a property is changed within an item.
    /// </summary>
    public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;

    public ObservableCollectionEx() : base()
    { }

    public ObservableCollectionEx(List<T> list) : base(list)
    {
        ObserveAll();
    }

    public ObservableCollectionEx(IEnumerable<T> enumerable) : base(enumerable)
    {
        ObserveAll();
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Replace)
        {
            foreach (T item in e.OldItems.NeverNull())
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Replace)
        {
            foreach (T item in e.NewItems.NeverNull())
            {
                if (item != null)
                {
                    item.PropertyChanged += ChildPropertyChanged;
                }
            }
        }
        OnBatchChange();
        base.OnCollectionChanged(e);
    }

    protected void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
    {
        ItemPropertyChanged?.Invoke(this, e);
    }

    protected void OnItemPropertyChanged(int index, PropertyChangedEventArgs e)
    {
        OnItemPropertyChanged(new ItemPropertyChangedEventArgs(index, e));
    }

    protected override void ClearItems()
    {
        foreach (T item in Items)
        {
            if (item != null)
            {
                item.PropertyChanged -= ChildPropertyChanged;
            }
        }

        base.ClearItems();
    }

    private void ObserveAll()
    {
        foreach (T item in Items)
        {
            if (item != null)
            {
                item.PropertyChanged += ChildPropertyChanged;
            }
        }
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the ObservableCollectionEx.
    /// </summary>
    public void AddRange(IEnumerable<T> collection)
        => AddRange(collection, false);

    /// <summary>
    /// Adds the elements of the specified collection to the end of the ObservableCollectionEx.
    /// </summary>
    public void AddRange(IEnumerable<T> collection, bool clearFirst)
    {
        _batchChangeSuppressed = true;
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
        bool changed = false;
        if (Count > 0 && clearFirst)
        {
            Clear();
            changed = true;
        }
        int index = Count;
        foreach (T i in collection)
        {
            //although this fires a lot of events, it's the best way to reliably be observed, apparently :-(
            InsertItem(index, i);
            changed = true;
            index++;
        }
        _batchChangeSuppressed = false;
        if (changed)
        {
            OnBatchChange();
        }
    }

    private void ChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is null)
        {
            throw new ArgumentNullException(nameof(sender));
        }
        T typedSender = (T)sender;
        int i = Items.IndexOf(typedSender);

        if (i < 0)
        {
            throw new ArgumentException("Received property notification from item not in collection");
        }

        OnItemPropertyChanged(i, e);
    }
}

/// <summary>
/// Provides data for the <see cref="ObservableCollectionEx{T}.ItemPropertyChanged"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
/// </remarks>
/// <param name="index">The index in the collection of changed item.</param>
/// <param name="name">The name of the property that changed.</param>
public class ItemPropertyChangedEventArgs(int index, string? name) : PropertyChangedEventArgs(name)
{
    /// <summary>
    /// Gets the index in the collection for which the property change has occurred.
    /// </summary>
    /// <value>
    /// Index in parent collection.
    /// </value>
    public int CollectionIndex { get; } = index;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : this(index, args.PropertyName)
    { }
}

