
namespace GameshowPro.Common.Model;

[method:JsonConstructor]
public class PingHostSettings(string? host, string? displayName, RemoteServiceSettings? remoteServiceSettings) : ObservableClass, IPingHostSettings
{
    public PingHostSettings() : this(null, null, null)
    { }

    private string _host = host ?? string.Empty;
    [DataMember, DefaultValue("localhost")]
    public string Host
    {
        get { return _host; }
        set { SetProperty(ref _host, value); }
    }

    private string _displayName = displayName ?? string.Empty;

    /// <summary>
    /// A name which can be used shown on the UI to distinguish this device instance from another of the same type.
    /// </summary>
    [DataMember]
    public string DisplayName
    {
        get { return _displayName; }
        set { SetProperty(ref _displayName, value); }
    }

    [DataMember]
    public IRemoteServiceSettings RemoteServiceSettings { get; } = remoteServiceSettings ?? new();
}

