using System.Net.NetworkInformation;

namespace Barjonas.Common.Model;

public class PingHost : ObservableClass, IRemoteService
{
    private readonly static TimeSpan s_interval = TimeSpan.FromSeconds(5);
    private readonly CancellationToken _cancellationToken;
    private readonly AutoResetEvent _settingChange = new(false);
    public PingHost(PingHostSettings settings, CancellationToken cancellationToken)
    {
        Settings = settings;
        settings.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.Host):
                    _settingChange.Set();
                    break;
            }
        };
        _cancellationToken = cancellationToken;
        ServiceState = new("Ping");
        _ = Task.Run(UpdateLoop, cancellationToken);
    }
    public PingHostSettings Settings { get; }

    private async Task UpdateLoop()
    {
        Ping pingSender = new();
        PingOptions options = new()
        {
            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            DontFragment = true
        };

        // Create a buffer of 32 bytes of data to be transmitted.
        string data = Enumerable.Repeat('a', 32).ToArray().ToString()!;
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        int timeout = 120;

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
                        PingReply reply = await pingSender.SendPingAsync(Settings.Host, timeout, buffer, options);
                        ServiceState.AggregateState = reply.Status == IPStatus.Success ? RemoteServiceStates.Connected : RemoteServiceStates.Warning;
                        ServiceState.Detail = reply.Status == IPStatus.Success ? $"Reply time {reply.RoundtripTime}ms" : "No reply";
                        if (reply.Status == IPStatus.Success)
                        {
                            LastPingTime = DateTime.UtcNow;
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
