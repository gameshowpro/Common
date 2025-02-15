namespace GameshowPro.Common.Model;

/// <summary>
/// Implementors use MDNS to maintain a list of other MDNS services with the same type as the current instance.
/// This allows an interface to UIs to bring this to the attention of users.
/// </summary>
public interface IMdnsConflictMonitor
{
    ImmutableArray<IMdnsConflictingService> ConflictingServices { get; }
}

/// <summary>
/// A single host on which a conflicting service was found, along with a list of advertised IP addresses.
/// </summary>
public interface IMdnsConflictingService
{
    string HostName { get; }
    ImmutableArray<IPAddress> Addresses { get; }
}
