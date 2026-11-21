namespace GameshowPro.Common.Model;

/// <summary>
/// Monitors a collection of remote services and provides dynamic grouping for monitoring UI purposes.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public class RemoteServiceManager : ObservableClass
{
    private readonly UngroupedServiceCollection _orphanServices;
    private readonly List<IRemoteServiceCollection> _serviceCollections;
    private HashSet<IRemoteService> _serviceChangeSubscriptions = [];
    private HashSet<IRemoteServiceCollection> _serviceCollectionChangeSubscriptions = [];

    /// <summary>
    /// Creates a new manager over services and collections.
    /// </summary>
    /// <param name="settings">User-configurable settings.</param>
    /// <param name="remoteServices">Loose services not initially in a collection.</param>
    /// <param name="remoteServiceCollections">Pre-existing service collections.</param>
    /// <param name="uiTemplatesByType">WPF template resource keys by type.</param>
    /// <remarks>Docs added by AI.</remarks>
    public RemoteServiceManager(RemoteServiceManagerSettings settings, IEnumerable<IRemoteService> remoteServices, IEnumerable<IRemoteServiceCollection> remoteServiceCollections, IEnumerable<KeyValuePair<Type, string>> uiTemplatesByType)
    {
        Settings = settings;
        _serviceCollections = [.. remoteServiceCollections];
        _orphanServices = new(remoteServices);
        _serviceCollections.Add(_orphanServices);
        ServiceCollectionsListAddedOrRemoved();
#if WPF
        DataTemplateSelector = new RemoteServiceDataTemplateSelector(uiTemplatesByType.ToFrozenDictionary());
#endif
    }

    /// <summary>The current settings; updating triggers regrouping.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public RemoteServiceManagerSettings Settings
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                UpdateGroups();
            }
        }
    }

#if WPF
    public DataTemplateSelector DataTemplateSelector { get; }
#endif
    /// <summary>Adds a single service to the ungrouped collection.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void AddService(IRemoteService service)
    {
        _orphanServices.Add(service);
    }

    /// <summary>Adds a service collection and subscribes to its changes.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public void AddServices(IRemoteServiceCollection services)
    {
        _serviceCollections.Add(services);
        ServiceCollectionsListAddedOrRemoved();
    }

    /// <summary>
    /// Handle a service collections list being added or removed.
    /// </summary>
    private void ServiceCollectionsListAddedOrRemoved()
    {
        bool changed = false;
        HashSet<IRemoteServiceCollection> newList = [.. _serviceCollections];
        // Unsubscribe from collections that are no longer in the list
        foreach (IRemoteServiceCollection collection in _serviceCollectionChangeSubscriptions.Except(newList))
        {
            collection.RemoteServiceCollectionChanged -= Collection_RemoteServiceCollectionChanged;
            changed = true;
        }
        // Subscribe to collections that are new to the list
        foreach (IRemoteServiceCollection collection in newList.Except(_serviceCollectionChangeSubscriptions))
        {
            collection.RemoteServiceCollectionChanged += Collection_RemoteServiceCollectionChanged;
            changed = true;
        }
        if (changed)
        {
            _serviceCollectionChangeSubscriptions = newList;
            RemoteServiceAddedOrRemoved();
        }
    }

    private void RemoteServiceAddedOrRemoved()
    {
        HashSet<IRemoteService> newList = [.. _serviceCollections.SelectMany(static s => s.Services)];
        bool changed = false;
        // Unsubscribe from services that are no longer in the list
        foreach (IRemoteService service in _serviceChangeSubscriptions.Except(newList))
        {
            if (service.RemoteServiceSettings is not null)
            {
                service.RemoteServiceSettings.MonitorUiGroupChanged -= Settings_MonitorUiGroupChanged;
            }
            if (service is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged -= Service_PropertyChanged;
            }
            changed = true;
        }
        // Subscribe to services that are new to the list
        foreach (IRemoteService service in newList.Except(_serviceChangeSubscriptions))
        {
            if (service.RemoteServiceSettings is not null)
            {
                service.RemoteServiceSettings.MonitorUiGroupChanged += Settings_MonitorUiGroupChanged;
            }
            if (service is INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += Service_PropertyChanged;
            }
            changed = true;
        }
        if (changed)
        {
            _serviceChangeSubscriptions = newList;
            UpdateGroups();
        }
    }

    private void Collection_RemoteServiceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => RemoteServiceAddedOrRemoved();

    private void Service_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(IRemoteService.IsConfigured))
        {
            UpdateGroups();
        }
    }

    private readonly Lock _updateLock = new();
    private bool _updatingGroups;
    private bool _updateGroupsPending;

    /// <summary>
    /// Rebuilds <see cref="MonitorUiGroups"/>. A request arriving while a rebuild is running (reentrant or from another thread)
    /// is not dropped: the running rebuild repeats once it finishes.
    /// </summary>
    private void UpdateGroups()
    {
        lock (_updateLock)
        {
            if (_updatingGroups)
            {
                _updateGroupsPending = true;
                return;
            }
            _updatingGroups = true;
        }
        try
        {
            while (true)
            {
                RebuildGroups();
                lock (_updateLock)
                {
                    if (!_updateGroupsPending)
                    {
                        _updatingGroups = false;
                        return;
                    }
                    _updateGroupsPending = false;
                }
            }
        }
        catch
        {
            lock (_updateLock)
            {
                _updatingGroups = false;
                _updateGroupsPending = false;
            }
            throw;
        }
    }

    private void RebuildGroups()
    {
        // Group, index, name slots and MonitorUiOrder are all computed over every service, so hiding a service never shifts anything.
        ImmutableArray<RemoteServiceGroup> allGroups = [..
            _serviceChangeSubscriptions
            .GroupBy(s => s.RemoteServiceSettings?.MonitorUiGroup ?? -1)
            .Select(g => new RemoteServiceGroup(g.Key, [.. g.OrderBy(s => s.RemoteServiceSettings?.MonitorUiOrder)]))
            .OrderBy(g => g.Index)
        ];
        allGroups.SetIndices();
        Settings.Groups = Settings.Groups.ImmutableArrayEnsureCountAndIndices(allGroups.Length, allGroups.Length, i => new RemoteServiceGroupSettings());
        allGroups.ForEachWithIndex((g, i) =>
        {
            g.Settings = Settings.Groups[i];
            g.Items.ForEachWithIndex(SetMonitorUiOrder);
        });
        static void SetMonitorUiOrder(IRemoteService service, int order)
        {
            service.RemoteServiceSettings?.MonitorUiOrder = order * 2;
        }

        // Publish only configured services, and only groups that still have one. Each visible group keeps its unfiltered Index and Settings.
        List<RemoteServiceGroup> visibleGroups = [];
        foreach (RemoteServiceGroup group in allGroups)
        {
            ImmutableArray<IRemoteService> visibleItems = [.. group.Items.Where(static s => s.IsConfigured)];
            if (visibleItems.Length > 0)
            {
                visibleGroups.Add(new RemoteServiceGroup(group.Index, visibleItems) { Settings = group.Settings });
            }
        }
        // Leave MonitorUiGroups alone when nothing visible changed, so the UI does not rebuild its visual tree for nothing.
        if (MonitorUiGroups.IsDefault || !SameVisibleGroups(MonitorUiGroups, visibleGroups))
        {
            MonitorUiGroups = [.. visibleGroups];
        }
    }

    private static bool SameVisibleGroups(ImmutableArray<RemoteServiceGroup> current, List<RemoteServiceGroup> candidate)
    {
        if (current.Length != candidate.Count)
        {
            return false;
        }
        for (int i = 0; i < candidate.Count; i++)
        {
            if (current[i].Index != candidate[i].Index
                || !ReferenceEquals(current[i].Settings, candidate[i].Settings)
                || !current[i].Items.SequenceEqual(candidate[i].Items))
            {
                return false;
            }
        }
        return true;
    }

    private void Settings_MonitorUiGroupChanged(object? sender, EventArgs args)
        => UpdateGroups();

    /// <summary>Computed UI groups for monitoring.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public ImmutableArray<RemoteServiceGroup> MonitorUiGroups
    {
        get;
        private set => _ = SetProperty(ref field, value);
    }

    private class UngroupedServiceCollection(IEnumerable<IRemoteService> remoteServices) : IRemoteServiceCollection
    {
        public readonly List<IRemoteService> _services = [.. remoteServices];
        public IEnumerable<IRemoteService> Services => _services;
        public event NotifyCollectionChangedEventHandler? RemoteServiceCollectionChanged;

        public void Add(IRemoteService service)
        {
            _services.Add(service);
            RemoteServiceCollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, service, null, _services.Count - 1));
        }
    }

#if WPF
    private class RemoteServiceDataTemplateSelector(FrozenDictionary<Type, string> templatePathsByType) : DataTemplateSelector
    {
        private readonly FrozenDictionary<Type, string> _templatePathsByType = templatePathsByType;
        private readonly Dictionary<Type, DataTemplate?> _templatesByType = [];
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (_templatesByType.TryGetValue(item.GetType(), out DataTemplate? template))
            {
                return template;
            }
            if (container is FrameworkElement element && _templatePathsByType.TryGetValue(item.GetType(), out string? templatePath))
            {
                if (element.TryFindResource(templatePath) is DataTemplate dataTemplate)
                {
                    _templatesByType[item.GetType()] = dataTemplate;
                    return dataTemplate;
                }
                _templatesByType[item.GetType()] = null; // Never try to load this template again
            }
            return null;
        }
    }
#endif
}
