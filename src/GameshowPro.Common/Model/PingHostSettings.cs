
namespace GameshowPro.Common.Model;

[method:JsonConstructor]
public class PingHostSettings(string? host, string? displayName, RemoteServiceSettings? remoteServiceSettings) : ObservableClass, IPingHostSettings
{
    public PingHostSettings() : this(null, null, null)
    { }

    [DataMember, DefaultValue("localhost")]
    public string Host
    {
        get;
        set { SetProperty(ref field, value); }
    } = host ?? string.Empty;

    /// <summary>
    /// A name which can be used shown on the UI to distinguish this device instance from another of the same type.
    /// </summary>
    [DataMember]
    public string DisplayName
    {
        get;
        set { SetProperty(ref field, value); }
    } = displayName ?? string.Empty;

    [DataMember]
    public IRemoteServiceSettings RemoteServiceSettings { get; } = remoteServiceSettings ?? new();
}

