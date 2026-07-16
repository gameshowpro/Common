namespace GameshowPro.Common.Model;

public class PingHost : ObservableClass, IRemoteService
{
    private static readonly TimeSpan s_interval = TimeSpan.FromSeconds(5);
    private readonly CancellationToken _cancellationToken;
    private readonly AutoResetEvent _settingChange = new(false);
    private readonly ILogger _logger;
    public PingHost(IPingHostSettings settings, ILogger logger, CancellationToken cancellationToken)
    {
        Settings = settings;
        _logger = logger;
        ServiceState = new("Ping");
        settings.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.Host):
                    ServiceState.AggregateState = RemoteServiceStates.Disconnected;
                    ServiceState.Detail = "In progress";
                    _ = _settingChange.Set();
                    break;
            }
        };
        _cancellationToken = cancellationToken;
        _ = Task.Run(UpdateLoop, cancellationToken);
    }
    public IPingHostSettings Settings { get; }
    IRemoteServiceSettings IRemoteService.RemoteServiceSettings => Settings.RemoteServiceSettings;

    private async Task UpdateLoop()
    {
        var waitHandles = new WaitHandle[] { _cancellationToken.WaitHandle, _settingChange };
        int handle;
        while (!_cancellationToken.IsCancellationRequested)
        {
            handle = WaitHandle.WaitAny(waitHandles, s_interval);
            switch (handle)
            {
                case 1:
                case WaitHandle.WaitTimeout:
                    if (string.IsNullOrWhiteSpace(Settings.Host))
                    {
                        ServiceState.AggregateState = RemoteServiceStates.Disconnected;
                        ServiceState.Detail = "No host specified";
                    }
                    else
                    {
                        PingHostNameResult result = await PingClient.SendPing(Settings.Host, _logger, _cancellationToken);
                        if (result.MinimumRoundtripTime.HasValue)
                        {
                            ServiceState.AggregateState = RemoteServiceStates.Connected;
                            ServiceState.Detail = $"Reply time {result.MinimumRoundtripTime.Value.TotalMilliseconds}ms";
                            LastPingTime = DateTime.UtcNow;
                        }
                        else
                        {
                            ServiceState.AggregateState = RemoteServiceStates.Warning;
                            ServiceState.Detail = result.AddressResults.Length == 0 ? "Could not resolve host name" : "No reply";
                        }
                    }
                    break;
                case 0:
                    //_cancelling
                    return;
            }
        }
    }

    public ServiceState ServiceState { get; }
    public DateTime LastPingTime
    {
        get;
        private set => _ = SetProperty(ref field, value);
    }
}
