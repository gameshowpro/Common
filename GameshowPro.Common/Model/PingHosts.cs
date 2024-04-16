namespace GameshowPro.Common.Model;

public class PingHosts(IEnumerable<PingHostSettings> settings, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
{
    public ObservableCollection<PingHost> Items { get; } = new(settings.Select((s,i) => new PingHost(s, loggerFactory.CreateLogger($"{nameof(PingHost)}[{i}]"), cancellationToken)));
}
