namespace GameshowPro.Common.Model;

public class PingHosts(IEnumerable<IPingHostSettings> settings, ILoggerFactory loggerFactory, CancellationToken cancellationToken) : IRemoteServiceCollection
{
    event NotifyCollectionChangedEventHandler? IRemoteServiceCollection.RemoteServiceCollectionChanged
    {
        add => Items.CollectionChanged += value;
        remove => Items.CollectionChanged -= value;
    }

    public ObservableCollection<PingHost> Items { get; } = new(settings.Select((s, i) => new PingHost(s, loggerFactory.CreateLogger($"{nameof(PingHost)}[{i}]"), cancellationToken)));

    IEnumerable<IRemoteService> IRemoteServiceCollection.Services => Items;
}
