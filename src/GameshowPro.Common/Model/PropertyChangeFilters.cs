// (C) Barjonas LLC 2022
namespace GameshowPro.Common.Model;

/// <summary>
/// Represents a queued filter trigger consisting of the sender and property change args.
/// </summary>
/// <param name="sender">The event sender.</param>
/// <param name="args">The property changed event args.</param>
/// <remarks>Docs added by AI.</remarks>
public record FilterTriggerInstance(object sender, PropertyChangedEventArgs args);
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
    /// <param name="property">The name of the property which must change. If <see cref="string.Empty"/>, the condition will be met regardless of the property name in the change notification</param>
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
    internal readonly ILogger _logger;
    private readonly PropertyChangedEventHandler _handler;
    private readonly ImmutableList<INotifyPropertyChanged> _itemSenders; //Cannot use hashset because HashCodes are mutable
    private readonly ImmutableList<FrozenSet<string>> _notifyItemConditions;
    private readonly ImmutableList<INotifyCollectionChanged> _collectionSenders;
    private readonly ImmutableList<FrozenSet<string?>> _notifyCollectionConditions;
    private readonly List<FilterTriggerInstance> _pausedQueue = [];

    internal PropertyChangeFilter(PropertyChangedEventHandler action, IEnumerable<PropertyChangeCondition> conditions, bool invokeAfterConstruction, ILogger logger)
    {
        _logger = logger;
        _handler = action;
        ImmutableList<INotifyCollectionChanged>.Builder collectionSendersBuilder = ImmutableList.CreateBuilder<INotifyCollectionChanged>();
        List<HashSet<string?>> notifyCollectionConditions = [];
        ImmutableList<INotifyPropertyChanged>.Builder itemSendersBuilder = ImmutableList.CreateBuilder<INotifyPropertyChanged>();
        List<HashSet<string>> notifyItemConditions = [];
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
                            notifyCollectionConditions.Add([]);
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
                        notifyItemConditions.Add([]);
                    }
                    notifyItemConditions[senderIndex].Add(condition.Property);
                }
            }
        }
        _itemSenders = itemSendersBuilder.ToImmutable();
        _notifyItemConditions = [.. notifyItemConditions.Select(c => c.ToFrozenSet())];
        _collectionSenders = collectionSendersBuilder.ToImmutable();
        _notifyCollectionConditions = [.. notifyCollectionConditions.Select(c => c.ToFrozenSet())];
        //Note the property handlers are hooked up last, otherwise they may fire before non-nullable lists they consume have been set.
        AddEventHandlers();
        if (invokeAfterConstruction)
        {
            InvokeAll();
        }
    }

    /// <summary>
    /// Use the same lists that the event handlers will use. If they're not ready, we'll fail fast.
    /// </summary>
    private void AddEventHandlers()
    {
        foreach (INotifyPropertyChanged sender in _itemSenders)
        {
            if (sender is ObservableClass observable)
            {
               //avoid dispatcher
               observable.PropertyChangedOnOriginalThread += Sender_PropertyChanged;
            }
            else
            {
                sender.PropertyChanged += Sender_PropertyChanged;
            }
        }
        int senderIndex = 0;
        foreach (INotifyCollectionChanged sender in _collectionSenders)
        {
            sender.CollectionChanged += SenderCollection_CollectionChanged;
            if (_notifyCollectionConditions.ElementAtOrDefault(senderIndex)?.Count > 0 && sender is IItemPropertyChanged ipc)
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
            if (sender is ObservableClass observable)
            {
                observable.PropertyChangedOnOriginalThread -= Sender_PropertyChanged;
            }
            else
            {
                sender.PropertyChanged -= Sender_PropertyChanged;
            }
        }
        int senderIndex = 0;
        foreach (INotifyCollectionChanged sender in _collectionSenders)
        {
            sender.CollectionChanged -= SenderCollection_CollectionChanged;
            if (_notifyCollectionConditions.ElementAtOrDefault(senderIndex)?.Count > 0 && sender is IItemPropertyChanged ipc)
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
                InvokeOrEnqueue(sender, new PropertyChangedEventArgs(e.PropertyName));
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
                InvokeOrEnqueue(sender, new PropertyChangedEventArgs(e.Action.ToString()));
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
                if (senderIndex >= 0 &&
                    (
                        _notifyItemConditions[senderIndex].Contains(string.Empty) ||
                        (e.PropertyName is not null && _notifyItemConditions[senderIndex].Contains(e.PropertyName))
                    )
                )
                {
                    InvokeOrEnqueue(sender, e);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected condition encountered in property handler. Null states: sender={sender}, e={e}, _itemSenders={_itemSenders}, _notifyItemConditions={_notifyItemConditions}",
                sender is null,
                e is null,
                _itemSenders is null,
                _notifyItemConditions is null
            );
        }
    }

    internal void InvokeAll()
        => InvokeOrEnqueue(this, new PropertyChangedEventArgs(string.Empty));

    internal void Release()
        => RemoveEventHandlers();

    private void InvokeOrEnqueue(object sender, PropertyChangedEventArgs args)
    {
        if (Paused)
        {
            _pausedQueue.Add(new (sender, args));
        }
        else
        {
            _handler.Invoke(sender, args);
        }
    }

    public bool Paused { get; private set; }

    /// <summary>
    /// Stop firing the handler until after <see cref="Resume"/> is called.
    /// </summary>
    public void Pause()
        => Paused = true;

    /// <summary>
    /// Return all triggers held back from handler since <see cref="Pause"/> was called, then resume firing the handler.
    /// </summary>
    public ImmutableList<FilterTriggerInstance> Resume()
    {
        ImmutableList<FilterTriggerInstance> queue = [.. _pausedQueue];
        Paused = false;
        _pausedQueue.Clear();
        return queue;
    }
}

/// <summary>
/// Manages a set of <see cref="PropertyChangeFilter"/> instances and provides convenience methods.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public class PropertyChangeFilters
{
    private static ILoggerFactory? s_loggerFactory;
    /// <summary>
    /// Assigns the static logger factory used by default constructors.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use.</param>
    /// <remarks>Docs added by AI.</remarks>
    public static void AssignLoggerFactory(ILoggerFactory loggerFactory)
    {
        if (s_loggerFactory != null)
        {
            throw new InvalidOperationException("Logger factory is already assigned.");
        }
        s_loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates a new instance using a logger from the assigned factory.
    /// </summary>
    /// <param name="loggerNamePrefix">The first part of the logger name to distinguish this instance.</param>
    /// <exception cref="InvalidOperationException">Thrown if no logger factory has been assigned via <see cref="AssignLoggerFactory(ILoggerFactory)"/>.</exception>
    /// <remarks>Docs added by AI.</remarks>
    public PropertyChangeFilters(string loggerNamePrefix)
    {
        if (s_loggerFactory is null)
        {
            throw new InvalidOperationException($"{nameof(AssignLoggerFactory)} must be called before calling this constructor.");
        }
        _logger = s_loggerFactory.CreateLogger($"{loggerNamePrefix}/{nameof(PropertyChangeFilters)}");
    }

    /// <summary>
    /// Creates a new instance using the provided logger.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <remarks>Docs added by AI.</remarks>
    public PropertyChangeFilters(ILogger logger)
    {
        _logger = logger;
    }

    private readonly ILogger _logger;
    private readonly List<PropertyChangeFilter> _filters = [];

    /// <summary>
    /// Adds a new filter and optionally invokes the handler immediately after construction.
    /// </summary>
    /// <param name="handler">The delegate to invoke when conditions are met.</param>
    /// <param name="invokeAfterConstruction">Whether to invoke the handler immediately.</param>
    /// <param name="conditions">The conditions that must be met.</param>
    /// <returns>The created filter.</returns>
    /// <remarks>Docs added by AI.</remarks>
    [return: NotNullIfNotNull(nameof(conditions))]
    public PropertyChangeFilter AddFilter(PropertyChangedEventHandler handler, bool invokeAfterConstruction, params PropertyChangeCondition[] conditions)
        => AddFilter(handler, invokeAfterConstruction, (IEnumerable<PropertyChangeCondition>)conditions);

    /// <summary>
    /// Adds a new filter and invokes the handler immediately after construction.
    /// </summary>
    /// <param name="handler">The delegate to invoke when conditions are met.</param>
    /// <param name="conditions">The conditions that must be met.</param>
    /// <returns>The created filter.</returns>
    /// <remarks>Docs added by AI.</remarks>
    [return: NotNullIfNotNull(nameof(conditions))]
    public PropertyChangeFilter AddFilter(PropertyChangedEventHandler handler, params PropertyChangeCondition[] conditions)
        => AddFilter(handler, (IEnumerable<PropertyChangeCondition>)conditions);

    /// <summary>
    /// Adds a new filter and invokes the handler immediately after construction.
    /// </summary>
    /// <param name="handler">The delegate to invoke when conditions are met.</param>
    /// <param name="conditions">The conditions that must be met, or null to skip.</param>
    /// <returns>The created filter or null if conditions is null.</returns>
    /// <remarks>Docs added by AI.</remarks>
    [return: NotNullIfNotNull(nameof(conditions))]
    public PropertyChangeFilter? AddFilter(PropertyChangedEventHandler handler, IEnumerable<PropertyChangeCondition>? conditions)
        => AddFilter(handler, true, conditions);

    /// <summary>
    /// Adds a new filter and optionally invokes the handler immediately after construction.
    /// </summary>
    /// <param name="handler">The delegate to invoke when conditions are met.</param>
    /// <param name="invokeAfterConstruction">Whether to invoke the handler immediately.</param>
    /// <param name="conditions">The conditions that must be met, or null to skip.</param>
    /// <returns>The created filter or null if conditions is null.</returns>
    /// <remarks>Docs added by AI.</remarks>
    [return:NotNullIfNotNull(nameof(conditions))]
    public PropertyChangeFilter? AddFilter(PropertyChangedEventHandler handler, bool invokeAfterConstruction, IEnumerable<PropertyChangeCondition>? conditions)
    {
        if (conditions != null)
        {
            PropertyChangeFilter filter = new (handler, conditions, invokeAfterConstruction, _logger);
            _filters.Add(filter);
            return filter;
        }
        return null;
    }

    /// <summary>
    /// Removes and releases all filters.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public void ClearFilters()
    {
        foreach (PropertyChangeFilter f in _filters)
        {
            f.Release();
        }
        _filters.Clear();
    }

    /// <summary>
    /// Invokes all filters' handlers regardless of state.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public void InvokeAll()
    {
        _filters.ForEach(f => f.InvokeAll());
    }

    /// <summary>
    /// Indicates whether any filters are registered.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool Any() => _filters.Count != 0;
}

