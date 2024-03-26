using System.Net.NetworkInformation;

namespace GameshowPro.Common.Model;

public record PingHostNamesResult(TimeSpan? MinimumRoundtripTime, ImmutableArray<PingHostNameResult> HostNameResults);
public record PingHostNameResult(string HostName, TimeSpan? MinimumRoundtripTime, ImmutableArray<PingAddressResult> AddressResults);
public record PingAddressResult(IPAddress IpAddress, TimeSpan? RoundtripTime);
public static class PingClient
{
    private static readonly PingOptions s_pingOptions = new()
    {
        DontFragment = true
    };
    private static readonly byte[] s_buffer = Encoding.ASCII.GetBytes(Enumerable.Repeat('a', 32).ToArray().ToString()!);
    private static readonly TimeSpan s_timeout = TimeSpan.FromMilliseconds(120);
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
            logger.LogInformation("Ping to {address} succeeded in {time}ms", ipAddress, reply.RoundtripTime);
            return new(ipAddress, TimeSpan.FromMilliseconds(reply.RoundtripTime));
        }
        else
        {
            logger.LogWarning("Ping to {address} failed with status {status}", ipAddress, reply.Status);
            return new(ipAddress, null);
        }
    }

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

    public static async Task<PingHostNamesResult> SendPing(IEnumerable<string> hostNames, ILogger logger, CancellationToken cancellationToken)
    {
        ImmutableArray<PingHostNameResult> results = [.. await Task.WhenAll(hostNames.Select(hostName => SendPing(hostName, logger, cancellationToken)))];
        return new(results.Select(r => r.MinimumRoundtripTime).MinOrDefault(), results);
    }
}
