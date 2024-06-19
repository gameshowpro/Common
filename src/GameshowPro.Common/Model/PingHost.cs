namespace GameshowPro.Common.Model;

public class PingHost : ObservableClass, IRemoteService
{
    private readonly static TimeSpan s_interval = TimeSpan.FromSeconds(5);
    private readonly CancellationToken _cancellationToken;
    private readonly AutoResetEvent _settingChange = new(false);
    private readonly ILogger _logger;
    public PingHost(IPingHostSettings settings, ILogger logger,  CancellationToken cancellationToken)
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
                    _settingChange.Set();
                    break;
            }
        };
        _cancellationToken = cancellationToken;
        _ = Task.Run(UpdateLoop, cancellationToken);
    }
    public IPingHostSettings Settings { get; }
    IRemoteServiceSettings IRemoteService.Settings => Settings.RemoteServiceSettings;

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
                        PingHostNameResult result =  await PingClient.SendPing(Settings.Host, _logger, _cancellationToken);
                        if (result.MinimumRoundtripTime.HasValue)
                        {
                            ServiceState.AggregateState = RemoteServiceStates.Connected;
                            ServiceState.Detail = $"Reply time {result.MinimumRoundtripTime.Value.TotalMilliseconds}ms";
                            LastPingTime = DateTime.UtcNow;
                        }
                        else
                        {
                            ServiceState.AggregateState = RemoteServiceStates.Warning;
                            if (result.AddressResults.Length == 0)
                            {
                                ServiceState.Detail = "Could not resolve host name";
                            }
                            else
                            {
                                ServiceState.Detail = "No reply";
                            }
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

    private DateTime _lastPingTime;
    public DateTime LastPingTime
    {
        get => _lastPingTime;
        private set => _ = SetProperty(ref _lastPingTime, value);
    }
}
