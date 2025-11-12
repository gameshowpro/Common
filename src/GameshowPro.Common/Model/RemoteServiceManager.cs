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
        _settings = settings;
        _serviceCollections = [.. remoteServiceCollections];
        _orphanServices = new(remoteServices);
        _serviceCollections.Add(_orphanServices);
        ServiceCollectionsListAddedOrRemoved();
#if WPF
        DataTemplateSelector = new RemoteServiceDataTemplateSelector(uiTemplatesByType.ToFrozenDictionary());
#endif
    }

    private RemoteServiceManagerSettings _settings;
    /// <summary>The current settings; updating triggers regrouping.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public RemoteServiceManagerSettings Settings
    {
        get => _settings;
        set 
        {
            if (SetProperty(ref _settings, value))
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
        bool changed = false;
        HashSet<IRemoteService> newList = [.. _serviceCollections.SelectMany(s => s.Services)];
        // Unsubscribe from services that are no longer in the list
        foreach (IRemoteService service in _serviceChangeSubscriptions.Except(newList))
        {
            if (service.RemoteServiceSettings != null)
            {
                service.RemoteServiceSettings.MonitorUiGroupChanged -= Settings_MonitorUiGroupChanged;
                changed = true;
            }
        }
        // Subscribe to services that are new to the list
        foreach (IRemoteService service in newList.Except(_serviceChangeSubscriptions))
        {
            if (service.RemoteServiceSettings != null)
            {
                service.RemoteServiceSettings.MonitorUiGroupChanged += Settings_MonitorUiGroupChanged;
                changed = true;
            }
        }
        if (changed)
        {
            _serviceChangeSubscriptions = newList;
            UpdateGroups();
        }
    }

    private void Collection_RemoteServiceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => RemoteServiceAddedOrRemoved();

    private bool _updatingGroups;
    private void UpdateGroups()
    {
        if (_updatingGroups)
        {
            return;
        }
        _updatingGroups = true;
        MonitorUiGroups = [.. 
            _serviceChangeSubscriptions
            .GroupBy(s => s.RemoteServiceSettings?.MonitorUiGroup ?? -1)
            .Select(g => new RemoteServiceGroup(g.Key, [.. g.OrderBy(s => s.RemoteServiceSettings?.MonitorUiOrder)]))
            .OrderBy(g => g.Index)
        ];
        MonitorUiGroups.SetIndices();
        Settings.Groups = Settings.Groups.ImmutableArrayEnsureCountAndIndices(MonitorUiGroups.Length, MonitorUiGroups.Length, i => new RemoteServiceGroupSettings());
        MonitorUiGroups.ForEachWithIndex((g, i) =>
        {
            g.Settings = Settings.Groups[i];
            g.Items.ForEachWithIndex(SetMonitorUiOrder);
        });
        static void SetMonitorUiOrder(IRemoteService service, int order)
        {
            if (service.RemoteServiceSettings != null)
            {  
                service.RemoteServiceSettings.MonitorUiOrder = order * 2;
            }
        }
        _updatingGroups = false;
    }

    private void Settings_MonitorUiGroupChanged(object? sender, EventArgs args)
        => UpdateGroups(); 

    private ImmutableArray<RemoteServiceGroup> _monitorUiGroups;
    /// <summary>Computed UI groups for monitoring.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public ImmutableArray<RemoteServiceGroup> MonitorUiGroups
    {
        get => _monitorUiGroups;
        private set => _ = SetProperty(ref _monitorUiGroups, value);
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
