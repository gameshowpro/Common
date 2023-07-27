// (C) Barjonas LLC 2022





namespace Barjonas.Common.Model;

public record PropertyChangeCondition
{
    internal INotifyPropertyChanged? Sender { get; }
    internal INotifyCollectionChanged? SenderCollection { get; }
    internal string? Property { get; }
    internal bool IsInvalid { get; }

    /// <summary>
    /// A condition dependent on <seealso cref="INotifyPropertyChanged.PropertyChanged"/> property changing on a sender with the given property name.
    /// </summary>
    /// <param name="sender">The sender to monitor for the change. If null, this condition will be ignored.</param>
    /// <param name="property">The name of the property which must change.</param>
    public PropertyChangeCondition(INotifyPropertyChanged? sender, string property)
    {
        Sender = sender;
        Property = property;
        SenderCollection = null;
        IsInvalid = sender is null;
    }

    /// <summary>
    /// A condition dependent on any <seealso cref="INotifyCollectionChanged.CollectionChanged"/> events.
    /// </summary>
    /// <param name="sender">The sender to monitor for the change. If null, this condition will be ignored.</param>
    public PropertyChangeCondition(INotifyCollectionChanged? sender)
    {
        Sender = null;
        Property = null;
        SenderCollection = sender;
        IsInvalid = sender is null;
    }

    /// <summary>
    /// A condition dependent on any <seealso cref="INotifyCollectionChanged.CollectionChanged"/> events or <seealso cref="IItemPropertyChanged.ItemPropertyChanged"/> events with the given property name.</summary>
    /// <param name="sender">The sender to monitor for the change. If null, this condition will be ignored.</param>
    /// <param name="property">The name of the property which must change.</param>
    public PropertyChangeCondition(INotifyCollectionChanged? sender, string property)
    {
        Sender = null;
        Property = property;
        SenderCollection = sender;
        IsInvalid = sender is null;
        if (sender is not IItemPropertyChanged)
        {
            throw new InvalidEnumArgumentException($"To used this overload, {nameof(sender)} must implement IItemPropertyChanged");
        }
    }
}

/// <summary>
/// This will execute a given delegate whenever one specified properties changes on one of the specified objects.
/// </summary>
public class PropertyChangeFilter
{
    internal static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private readonly PropertyChangedEventHandler _handler;
    private readonly ImmutableList<INotifyPropertyChanged> _itemSenders; //Cannot use hashset because HashCodes are mutable
    private readonly ImmutableList<ImmutableHashSet<string>> _notifyItemConditions;
    private readonly ImmutableList<INotifyCollectionChanged> _collectionSenders;
    private readonly ImmutableList<ImmutableHashSet<string?>> _notifyCollectionConditions;
    internal PropertyChangeFilter(PropertyChangedEventHandler action, IEnumerable<PropertyChangeCondition> conditions)
    {
        _handler = action;
        ImmutableList<INotifyCollectionChanged>.Builder collectionSendersBuilder = ImmutableList.CreateBuilder<INotifyCollectionChanged>();
        List<HashSet<string?>> notifyCollectionConditions = new();
        ImmutableList<INotifyPropertyChanged>.Builder itemSendersBuilder = ImmutableList.CreateBuilder<INotifyPropertyChanged>();
        List<HashSet<string>> notifyItemConditions = new();
        int senderIndex;
        foreach (PropertyChangeCondition condition in conditions)
        {
            if (!condition.IsInvalid)
            {
                //ensure only one even registration per sender, even if we are monitoring multiple properties
                if (condition.Sender is null)
                {
                    if (condition.SenderCollection is not null)
                    {
                        senderIndex = collectionSendersBuilder.IndexOf(condition.SenderCollection);
                        if (senderIndex < 0)
                        {
                            senderIndex = collectionSendersBuilder.Count;
                            collectionSendersBuilder.Add(condition.SenderCollection);
                            notifyCollectionConditions.Add(new());
                        }
                        notifyCollectionConditions[senderIndex].Add(condition.Property);
                    }
                }
                else if (condition.Property is not null)
                {
                    senderIndex = itemSendersBuilder.IndexOf(condition.Sender);
                    if (senderIndex < 0)
                    {
                        senderIndex = itemSendersBuilder.Count;
                        itemSendersBuilder.Add(condition.Sender);
                        notifyItemConditions.Add(new());
                    }
                    notifyItemConditions[senderIndex].Add(condition.Property);
                }
            }
        }
        _itemSenders = itemSendersBuilder.ToImmutable();
        _notifyItemConditions = notifyItemConditions.Select(c => c.ToImmutableHashSet()).ToImmutableList();
        _collectionSenders = collectionSendersBuilder.ToImmutable();
        _notifyCollectionConditions = notifyCollectionConditions.Select(c => c.ToImmutableHashSet()).ToImmutableList();
        //Note the property handlers are hooked up last, otherwise they may fire before non-nullable lists they consume have been set.
        AddEventHandlers();
        InvokeAll();
    }

    /// <summary>
    /// Use the same lists that the event handlers will use. If they're not ready, we'll fail fast.
    /// </summary>
    private void AddEventHandlers()
    {
        foreach (INotifyPropertyChanged sender in _itemSenders)
        {
            sender.PropertyChanged += Sender_PropertyChanged;
        }
        int senderIndex = 0;
        foreach (INotifyCollectionChanged sender in _collectionSenders)
        {
            sender.CollectionChanged += SenderCollection_CollectionChanged;
            if (_notifyCollectionConditions.ElementAtOrDefault(senderIndex)?.Any() == true && sender is IItemPropertyChanged ipc)
            {
                ipc.ItemPropertyChanged += Ipc_ItemPropertyChanged;
            }
            senderIndex++;
        }
    }

    private void RemoveEventHandlers()
    {
        foreach (INotifyPropertyChanged sender in _itemSenders)
        {
            sender.PropertyChanged -= Sender_PropertyChanged;
        }
        int senderIndex = 0;
        foreach (INotifyCollectionChanged sender in _collectionSenders)
        {
            sender.CollectionChanged -= SenderCollection_CollectionChanged;
            if (_notifyCollectionConditions.ElementAtOrDefault(senderIndex)?.Any() == true && sender is IItemPropertyChanged ipc)
            {
                ipc.ItemPropertyChanged -= Ipc_ItemPropertyChanged;
            }
            senderIndex++;
        }
    }

    private void Ipc_ItemPropertyChanged(object? sender, ItemPropertyChangedEventArgs e)
    {
        if (sender is INotifyCollectionChanged collectionSender)
        {
            int senderIndex = _collectionSenders.IndexOf(collectionSender);
            if (senderIndex >= 0 && _notifyCollectionConditions[senderIndex].Contains(e.PropertyName))
            {
                _handler?.Invoke(sender, new PropertyChangedEventArgs(e.PropertyName));
            }
        }
    }

    private void SenderCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not null && sender is INotifyCollectionChanged collectionSender)
        {
            int senderIndex = _collectionSenders.IndexOf(collectionSender);
            if (senderIndex >= 0 && _notifyCollectionConditions[senderIndex].Contains(null))
            {
                _handler?.Invoke(sender, new PropertyChangedEventArgs(e.Action.ToString()));
            }
        }
    }

    private void Sender_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (sender is INotifyPropertyChanged itemSender)
            {
                int senderIndex = _itemSenders.IndexOf(itemSender);
                if (senderIndex >= 0 && e.PropertyName is not null && _notifyItemConditions[senderIndex].Contains(e.PropertyName))
                {
                    _handler?.Invoke(sender, e);
                }
            }
        }
        catch (Exception ex)
        {
            s_logger.Fatal(ex, "Unexpected condition encountered in property handler. Null states: sender={sender}, e={e}, _itemSenders={_itemSenders}, _notifyItemConditions={_notifyItemConditions}",
                sender is null,
                e is null,
                _itemSenders is null,
                _notifyItemConditions is null
            );
        }
    }

    internal void InvokeAll()
        => _handler?.Invoke(this, new PropertyChangedEventArgs(string.Empty));

    internal void Release()
        => RemoveEventHandlers();
}

public class PropertyChangeFilters
{
    private readonly List<PropertyChangeFilter> _filters = new();
    public void AddFilter(PropertyChangedEventHandler handler, params PropertyChangeCondition[] conditions)
    {
        AddFilter(handler, (IEnumerable<PropertyChangeCondition>)conditions);
    }

    public void AddFilter(PropertyChangedEventHandler handler, IEnumerable<PropertyChangeCondition>? conditions)
    {
        if (conditions != null)
        {
            var filter = new PropertyChangeFilter(handler, conditions);
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

    public void InvokeAll()
    {
        _filters.ForEach(f => f.InvokeAll());
    }

    public bool Any() => _filters.Any();
}

