namespace GameshowPro.Common.Model;

public class PingHosts(IEnumerable<PingHostSettings> settings, ILogger logger, CancellationToken cancellationToken)
{
    public ObservableCollection<PingHost> Items { get; } = new(settings.Select(s => new PingHost(s, logger, cancellationToken)));
}
