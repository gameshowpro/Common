using System.Collections.Specialized;

namespace GameshowPro.Common.Test;

#if WPF
// The Windows configurations target a Windows-only TFM, so every test class is platform-specific (MSTEST0084).
[OSCondition(OperatingSystems.Windows)]
#endif
[TestClass]
public class TestRemoteServiceManager
{
    private sealed class FakeSettings(int group, int order) : IRemoteServiceSettings
    {
        private int _group = group;
        private int _order = order;
        public event EventHandler? MonitorUiGroupChanged;
        /// <summary>Invoked (inside the setter) whenever the order actually changes.</summary>
        public Action<int>? OnOrderChanged { get; set; }
        public int MonitorUiGroup
        {
            get => _group;
            set
            {
                if (_group != value)
                {
                    _group = value;
                    MonitorUiGroupChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public int MonitorUiOrder
        {
            get => _order;
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnOrderChanged?.Invoke(value);
                    MonitorUiGroupChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    private sealed class FakeService(string name, FakeSettings? settings, bool isConfigured = true) : ObservableClass, IRemoteService
    {
        public string Name { get; } = name;
        public ServiceState ServiceState { get; } = new(name);
        public FakeSettings? Settings { get; } = settings;
        public IRemoteServiceSettings? RemoteServiceSettings => Settings;
        public bool IsConfigured
        {
            get;
            set => _ = SetProperty(ref field, value);
        } = isConfigured;
        public override string ToString() => Name;
    }

    /// <summary>A service that does not implement <see cref="INotifyPropertyChanged"/> and relies on the default <see cref="IRemoteService.IsConfigured"/>.</summary>
    private sealed class PlainService : IRemoteService
    {
        public ServiceState ServiceState { get; } = new("Plain");
        public IRemoteServiceSettings? RemoteServiceSettings => null;
    }

    private sealed class FakeCollection(IEnumerable<IRemoteService> services) : IRemoteServiceCollection
    {
        private readonly List<IRemoteService> _services = [.. services];
        public IEnumerable<IRemoteService> Services => _services;
        public event NotifyCollectionChangedEventHandler? RemoteServiceCollectionChanged;
        public void Remove(IRemoteService service)
        {
            _ = _services.Remove(service);
            RemoteServiceCollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, service));
        }
    }

    private static RemoteServiceManager CreateManager(RemoteServiceManagerSettings? settings, params IRemoteService[] services)
        => new(settings ?? new RemoteServiceManagerSettings(), services, [], []);

    private static string[] Names(RemoteServiceGroup group) => [.. group.Items.Select(static s => s.ToString()!)];

    private static int CountMonitorUiGroupsChanges(RemoteServiceManager manager, Action action)
    {
        int count = 0;
        void Handler(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RemoteServiceManager.MonitorUiGroups))
            {
                _ = Interlocked.Increment(ref count);
            }
        }
        manager.PropertyChanged += Handler;
        action();
        manager.PropertyChanged -= Handler;
        return count;
    }

    [TestMethod]
    public void UnconfiguredService_IsAbsentFromMonitorUiGroups()
    {
        FakeService a = new("A", new(0, 0));
        FakeService b = new("B", new(0, 2), isConfigured: false);
        RemoteServiceManager manager = CreateManager(null, a, b);

        Assert.AreEqual(1, manager.MonitorUiGroups.Length);
        CollectionAssert.AreEqual(new[] { "A" }, Names(manager.MonitorUiGroups[0]));
    }

    [TestMethod]
    public void DefaultInterfaceImplementation_IsConfigured()
    {
        PlainService plain = new();
        Assert.IsTrue(((IRemoteService)plain).IsConfigured);
    }

    [TestMethod]
    public void GroupWithNoConfiguredServices_IsAbsent()
    {
        FakeService a = new("A", new(0, 0), isConfigured: false);
        FakeService b = new("B", new(1, 0));
        RemoteServiceManager manager = CreateManager(null, a, b);

        Assert.AreEqual(1, manager.MonitorUiGroups.Length);
        CollectionAssert.AreEqual(new[] { "B" }, Names(manager.MonitorUiGroups[0]));
    }

    [TestMethod]
    public void Flip_RaisesOneMonitorUiGroupsChange_AndServiceReturnsToOriginalGroup()
    {
        FakeService a1 = new("A1", new(0, 0));
        FakeService a2 = new("A2", new(0, 2), isConfigured: false);
        FakeService b = new("B", new(1, 0));
        RemoteServiceManager manager = CreateManager(null, a1, a2, b);
        CollectionAssert.AreEqual(new[] { "A1" }, Names(manager.MonitorUiGroups[0]));

        int shown = CountMonitorUiGroupsChanges(manager, () => a2.IsConfigured = true);
        Assert.AreEqual(1, shown, "Showing a service should publish exactly once.");
        Assert.AreEqual(2, manager.MonitorUiGroups.Length);
        CollectionAssert.AreEqual(new[] { "A1", "A2" }, Names(manager.MonitorUiGroups[0]));

        int hidden = CountMonitorUiGroupsChanges(manager, () => a2.IsConfigured = false);
        Assert.AreEqual(1, hidden, "Hiding a service should publish exactly once.");
        CollectionAssert.AreEqual(new[] { "A1" }, Names(manager.MonitorUiGroups[0]));
    }

    [TestMethod]
    public void Rebuild_WithNoVisibleChange_DoesNotRaiseMonitorUiGroupsChanged()
    {
        FakeService a = new("A", new(0, 0));
        FakeService hidden = new("H", new(0, 2), isConfigured: false);
        RemoteServiceManager manager = CreateManager(null, a, hidden);

        // Re-raising for a hidden service, and an unrelated property, must not republish.
        int changes = CountMonitorUiGroupsChanges(manager, () =>
        {
            hidden.IsConfigured = false;
            hidden.Settings!.MonitorUiGroup = 0;
            a.ServiceState.Detail = "x";
        });
        Assert.AreEqual(0, changes);
    }

    [TestMethod]
    public void HiddenEarlierGroup_DoesNotShiftLaterGroupIndexOrNames()
    {
        RemoteServiceManagerSettings settings = new();
        FakeService a = new("A", new(0, 0));
        FakeService b = new("B", new(1, 0));
        FakeService c = new("C", new(2, 0));
        RemoteServiceManager manager = CreateManager(settings, a, b, c);
        Assert.AreEqual(3, settings.Groups.Length);
        settings.Groups[0].Name = "First";
        settings.Groups[1].Name = "Second";
        settings.Groups[2].Name = "Third";

        a.IsConfigured = false;

        Assert.AreEqual(2, manager.MonitorUiGroups.Length);
        Assert.AreEqual(1, manager.MonitorUiGroups[0].Index);
        Assert.AreEqual(2, manager.MonitorUiGroups[1].Index);
        Assert.AreEqual("Second", manager.MonitorUiGroups[0].Settings!.Name);
        Assert.AreEqual("Third", manager.MonitorUiGroups[1].Settings!.Name);
        Assert.AreEqual(3, settings.Groups.Length, "Settings.Groups must not be truncated.");
        Assert.AreEqual("First", settings.Groups[0].Name, "The hidden group's name must be kept.");

        a.IsConfigured = true;
        Assert.AreEqual(3, manager.MonitorUiGroups.Length);
        Assert.AreEqual("First", manager.MonitorUiGroups[0].Settings!.Name);
        Assert.AreEqual(0, manager.MonitorUiGroups[0].Index);
    }

    [TestMethod]
    public void MonitorUiOrder_IsUnchangedByHiding()
    {
        FakeService a = new("A", new(0, 0));
        FakeService b = new("B", new(0, 2), isConfigured: false);
        FakeService c = new("C", new(0, 4));
        _ = CreateManager(null, a, b, c);

        Assert.AreEqual(0, a.Settings!.MonitorUiOrder);
        Assert.AreEqual(2, b.Settings!.MonitorUiOrder);
        Assert.AreEqual(4, c.Settings!.MonitorUiOrder);
    }

    [TestMethod]
    public void RemovedService_IsUnsubscribed()
    {
        FakeService a = new("A", new(0, 0));
        FakeService b = new("B", new(0, 2));
        FakeCollection collection = new([a, b]);
        RemoteServiceManager manager = new(new RemoteServiceManagerSettings(), [], [collection], []);
        CollectionAssert.AreEqual(new[] { "A", "B" }, Names(manager.MonitorUiGroups[0]));

        collection.Remove(b);
        CollectionAssert.AreEqual(new[] { "A" }, Names(manager.MonitorUiGroups[0]));

        int changes = CountMonitorUiGroupsChanges(manager, () =>
        {
            b.IsConfigured = false;
            b.IsConfigured = true;
            b.Settings!.MonitorUiGroup = 5;
        });
        Assert.AreEqual(0, changes, "A removed service must no longer drive rebuilds.");
        CollectionAssert.AreEqual(new[] { "A" }, Names(manager.MonitorUiGroups[0]));
    }

    [TestMethod]
    public void ServiceWithNullSettings_IsTrackedAndObserved()
    {
        FakeService a = new("A", settings: null, isConfigured: false);
        RemoteServiceManager manager = CreateManager(null, a);
        Assert.AreEqual(0, manager.MonitorUiGroups.Length);

        a.IsConfigured = true;
        Assert.AreEqual(1, manager.MonitorUiGroups.Length);
        CollectionAssert.AreEqual(new[] { "A" }, Names(manager.MonitorUiGroups[0]));

        a.IsConfigured = false;
        Assert.AreEqual(0, manager.MonitorUiGroups.Length);
    }

    [TestMethod]
    public void FlipRaisedFromInsideRebuild_IsNotLost()
    {
        FakeService a = new("A", new(0, 0));
        FakeService b = new("B", new(0, 5), isConfigured: false); // Rebuild renumbers B's order to 2, which fires the hook below.
        bool flipped = false;
        b.Settings!.OnOrderChanged = _ =>
        {
            if (!flipped)
            {
                flipped = true;
                b.IsConfigured = true; // Reentrant, while the rebuild is running.
            }
        };
        RemoteServiceManager manager = CreateManager(null, a, b);

        Assert.IsTrue(flipped);
        CollectionAssert.AreEqual(new[] { "A", "B" }, Names(manager.MonitorUiGroups[0]));
    }

    [TestMethod]
    public void FlipRaisedFromAnotherThreadDuringRebuild_IsNotLost()
    {
        FakeService a = new("A", new(0, 0));
        FakeService b = new("B", new(0, 5), isConfigured: false);
        FakeService c = new("C", new(0, 6), isConfigured: false);
        RemoteServiceManager manager = CreateManager(null, a, b, c);
        // The initial build renumbered orders to 0, 2, 4. Now change a group so the next rebuild renumbers again.
        bool flipped = false;
        a.Settings!.OnOrderChanged = newOrder =>
        {
            if (newOrder != 100 && !flipped) // Only the rebuild's renumbering sets another value; the test's own change below sets 100.
            {
                flipped = true;
                // Runs inside the rebuild. Flip from another thread and wait for it to return.
                Task.Run(() => c.IsConfigured = true).Wait(TimeSpan.FromSeconds(10));
            }
        };
        a.Settings.MonitorUiOrder = 100; // Triggers a rebuild, which renumbers A (now last) to 4 and fires the hook.

        Assert.IsTrue(flipped);
        CollectionAssert.AreEqual(new[] { "C", "A" }, Names(manager.MonitorUiGroups[0]));
    }
}
