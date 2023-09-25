namespace Barjonas.Common.Model;

public class PingHostSettings : ObservableClass
{
    public PingHostSettings() : this(null, null)
    { }

    public PingHostSettings(string? host, string? displayName)
    {
        _host = host ?? string.Empty;
        _displayName = displayName ?? string.Empty;
    }

    private string _host = "localhost";
    [JsonProperty, DefaultValue("localhost")]
    public string Host
    {
        get { return _host; }
        set { SetProperty(ref _host, value); }
    }

    private string _displayName;
    /// <summary>
    /// A name which can be used shown on the UI to distinguish this device instance from another of the same type.
    /// </summary>
    [JsonProperty]
    public string DisplayName
    {
        get { return _displayName; }
        set { SetProperty(ref _displayName, value); }
    }
}

