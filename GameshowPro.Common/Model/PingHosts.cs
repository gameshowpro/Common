namespace GameshowPro.Common.Model;

public class PingHosts(IEnumerable<PingHostSettings> settings, CancellationToken cancellationToken)
{
    public ObservableCollection<PingHost> Items { get; } = new(settings.Select(s => new PingHost(s, cancellationToken)));
}
