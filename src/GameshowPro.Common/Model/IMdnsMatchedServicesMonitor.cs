namespace GameshowPro.Common.Model;

/// <summary>
/// Implementors use MDNS to maintain a list of other MDNS services with the same type as the current instance.
/// This allows an interface to UIs to bring this to the attention of users.
/// </summary>
public interface IMdnsMatchedServicesMonitor : INotifyPropertyChanged
{
    /// <summary>
    /// Raised whenever a matched service is selected from the UI. 
    /// The object passed as a tag to the original button is passed to allow filtering when there are multiple items sharing the same monitor.
    /// </summary>
    event Action<object, IMdnsMatchedService>? ServiceWasSelected;
    ImmutableArray<IMdnsMatchedService> Services { get; }
}

/// <summary>
/// A single host on which a conflicting service was found, along with a list of advertised IP addresses.
/// </summary>
public interface IMdnsMatchedService
{
    IMdnsMatchedServicesMonitor Parent { get;  }
    string HostName { get; }
    int Port { get; }
    ImmutableArray<IPAddress> Addresses { get; }
}
