using System.Net.NetworkInformation;

namespace GameshowPro.Common.Model;

/// <summary>
/// Aggregated ping results for a set of host names.
/// </summary>
/// <param name="MinimumRoundtripTime">The minimum roundtrip time across all host names.</param>
/// <param name="HostNameResults">The per-host results.</param>
/// <remarks>Docs added by AI.</remarks>
public record PingHostNamesResult(TimeSpan? MinimumRoundtripTime, ImmutableArray<PingHostNameResult> HostNameResults);
/// <summary>
/// Aggregated ping results for a single host name possibly resolving to multiple addresses.
/// </summary>
/// <param name="HostName">The host name queried.</param>
/// <param name="MinimumRoundtripTime">The minimum roundtrip time across all resolved addresses.</param>
/// <param name="AddressResults">The results per IP address.</param>
/// <remarks>Docs added by AI.</remarks>
public record PingHostNameResult(string HostName, TimeSpan? MinimumRoundtripTime, ImmutableArray<PingAddressResult> AddressResults);
/// <summary>
/// The result of pinging a single IP address.
/// </summary>
/// <param name="IpAddress">The IP address that was pinged.</param>
/// <param name="RoundtripTime">The measured roundtrip time, or null if the ping failed.</param>
/// <remarks>Docs added by AI.</remarks>
public record PingAddressResult(IPAddress IpAddress, TimeSpan? RoundtripTime);
/// <summary>
/// Utility methods to send ICMP echo requests and aggregate their results.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public static class PingClient
{
    private static readonly PingOptions s_pingOptions = new()
    {
        DontFragment = true
    };
    private static readonly byte[] s_buffer = Encoding.ASCII.GetBytes(Enumerable.Repeat('a', 32).ToArray().ToString()!);
    private static readonly TimeSpan s_timeout = TimeSpan.FromMilliseconds(120);
    /// <summary>
    /// Sends a ping to the specified IP address.
    /// </summary>
    /// <param name="ipAddress">The address to ping.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="cancellationToken">Token to cancel the ping.</param>
    /// <returns>The result including the measured roundtrip time, or null if the ping failed.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public static async Task<PingAddressResult> SendPing(IPAddress ipAddress, ILogger logger, CancellationToken cancellationToken)
    {
        Ping _pingSender = new(); //Create a new instance each time in case concurrency is required.
        PingReply? reply;
        try
        {
            reply = await _pingSender.SendPingAsync(ipAddress, s_timeout, s_buffer, s_pingOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while pinging {address}", ipAddress);
            return new(ipAddress, null);
        }
        if (reply.Status == IPStatus.Success)
        {
            //logger.LogTrace("Ping to {address} succeeded in {time}ms", ipAddress, reply.RoundtripTime);
            return new(ipAddress, TimeSpan.FromMilliseconds(reply.RoundtripTime));
        }
        else
        {
            logger.LogWarning("Ping to {address} failed with status {status}", ipAddress, reply.Status);
            return new(ipAddress, null);
        }
    }

    /// <summary>
    /// Resolves a host name and pings all resolved addresses, returning an aggregate result.
    /// </summary>
    /// <param name="hostName">The host name or IP string to ping.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Aggregated results including the minimum roundtrip time across all addresses.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public static async Task<PingHostNameResult> SendPing(string hostName, ILogger logger, CancellationToken cancellationToken)
    {
        ImmutableArray<PingAddressResult> results;
        if (IPAddress.TryParse(hostName, out IPAddress? ipAddress))
        {
             results = [await SendPing(ipAddress, logger, cancellationToken)];
        }
        else
        {
            IPAddress[] addresses;
            try
            {
                addresses = await Dns.GetHostAddressesAsync(hostName, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while resolving {hostName}", hostName);
                return new (hostName, null, []);
            }

            results = [.. await Task.WhenAll(addresses.Select(address => SendPing(address, logger, cancellationToken)))];
        }
        return new(hostName, results.Select(r => r.RoundtripTime).MinOrDefault(), results);
    }

    /// <summary>
    /// Pings multiple host names in parallel and aggregates the results.
    /// </summary>
    /// <param name="hostNames">The host names to ping.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An aggregate with the minimum roundtrip across all hosts and their individual results.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public static async Task<PingHostNamesResult> SendPing(IEnumerable<string> hostNames, ILogger logger, CancellationToken cancellationToken)
    {
        ImmutableArray<PingHostNameResult> results = [.. await Task.WhenAll(hostNames.Select(hostName => SendPing(hostName, logger, cancellationToken)))];
        return new(results.Select(r => r.MinimumRoundtripTime).MinOrDefault(), results);
    }
}
