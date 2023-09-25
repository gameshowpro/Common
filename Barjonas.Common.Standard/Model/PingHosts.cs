namespace Barjonas.Common.Model;

public class PingHosts
{
    public PingHosts(IEnumerable<PingHostSettings> settings, CancellationToken cancellationToken)
    {
        Items = new(settings.Select(s => new PingHost(s, cancellationToken)));
    }

    public ObservableCollection<PingHost> Items { get; }
}
