using System.Collections.Concurrent;

namespace GameshowPro.Common.Blazor;

/// <summary>
/// Formats UTC instants in the visitor's browser time zone. The browser supplies the zone and the offset for each
/// instant, so this works in both WebAssembly and server-hosted Blazor, including dates across daylight saving changes.
/// Prime a batch before rendering a list; formatting cached values makes no JavaScript calls. The cache holds at most
/// 10,000 instants. Re-prime the currently retained list after updates to keep those instants available at capacity.
/// </summary>
public sealed class GspLocalTimeFormatter(IJSRuntime js) : IAsyncDisposable
{
    private const int MaxCachedInstants = 10000;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ConcurrentDictionary<long, int> _offsets = new();
    private readonly Queue<long> _oldestFirst = new();
    private IJSObjectReference? _module;
    private bool _disposed;

    /// <summary>The browser's IANA time zone identifier, when known.</summary>
    public string? TimeZoneId { get; private set; }

    /// <summary>Loads the browser's offset, in minutes east of UTC, for each instant not already cached.</summary>
    public async Task PrimeAsync(IEnumerable<DateTimeOffset> instants)
    {
        ArgumentNullException.ThrowIfNull(instants);
        await _gate.WaitAsync();
        try
        {
            if (_disposed)
            {
                return;
            }
            DateTimeOffset[] requested = [.. instants.Select(value => value.ToUniversalTime())
                .DistinctBy(value => value.UtcTicks)];
            HashSet<long> requestedTicks = [.. requested.Select(value => value.UtcTicks)];
            DateTimeOffset[] missing = [.. requested.Where(value => !_offsets.ContainsKey(value.UtcTicks))];
            if (TimeZoneId is not null && missing.Length == 0)
            {
                return;
            }

            _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./_content/GameshowPro.Common.Blazor/gsp-local-time.js");
            string[] utc = [.. missing.Select(value => value.ToString("O", CultureInfo.InvariantCulture))];
            string[]? data = await _module.InvokeAsync<string[]>("describe", (object)utc);
            if (data is null || data.Length != missing.Length + 1)
            {
                throw new InvalidOperationException("The browser did not return time zone offsets for every instant.");
            }
            TimeZoneId = string.IsNullOrWhiteSpace(data[0]) ? "browser local time" : data[0];
            for (int i = 0; i < missing.Length; i++)
            {
                _offsets[missing[i].UtcTicks] = int.Parse(data[i + 1], CultureInfo.InvariantCulture);
                _oldestFirst.Enqueue(missing[i].UtcTicks);
            }
            // A view may re-prime its retained rows after paging. Evict unrelated instants first so
            // newly loaded rows do not displace rows that are still on screen in the same batch.
            int scanBudget = _oldestFirst.Count;
            while (_oldestFirst.Count > MaxCachedInstants)
            {
                long oldest = _oldestFirst.Dequeue();
                if (requestedTicks.Contains(oldest) && scanBudget-- > 0)
                {
                    _oldestFirst.Enqueue(oldest);
                }
                else
                {
                    _offsets.TryRemove(oldest, out _);
                }
            }
        }
        catch (Exception)
        {
            // Interop may be unavailable during prerender, after disconnect, or in restricted browsers.
            TimeZoneId = null;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Formats a primed instant with its numeric UTC offset and, by default, the browser's time zone name.</summary>
    public string? FormatCached(DateTimeOffset instant, string pattern = "yyyy-MM-dd HH:mm:ss", bool includeZoneId = true)
    {
        if (!_offsets.TryGetValue(instant.UtcTicks, out int minutes))
        {
            return null;
        }
        DateTimeOffset local = instant.ToOffset(TimeSpan.FromMinutes(minutes));
        string offset = FormatOffset(minutes);
        string zone = includeZoneId && TimeZoneId is not null ? $" ({TimeZoneId})" : "";
        return local.ToString(pattern, CultureInfo.InvariantCulture) + " " + offset + zone;
    }

    /// <summary>Returns the numeric browser offset for a primed instant.</summary>
    public string? OffsetCached(DateTimeOffset instant)
        => _offsets.TryGetValue(instant.UtcTicks, out int minutes) ? FormatOffset(minutes) : null;

    private static string FormatOffset(int minutes)
    {
        int magnitude = Math.Abs(minutes);
        return $"UTC{(minutes < 0 ? '-' : '+')}{magnitude / 60:00}:{magnitude % 60:00}";
    }

    /// <summary>Primes and formats one instant; use <see cref="PrimeAsync"/> for lists.</summary>
    public async Task<string?> FormatAsync(DateTimeOffset instant, string pattern = "yyyy-MM-dd HH:mm:ss")
    {
        await PrimeAsync([instant]);
        return FormatCached(instant, pattern);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        // Taking the gate waits for any PrimeAsync that is still in flight, and makes later calls return without interop.
        await _gate.WaitAsync();
        try
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            IJSObjectReference? module = _module;
            _module = null;
            if (module is not null)
            {
                try
                {
                    await module.DisposeAsync();
                }
                catch (Exception)
                {
                    // The circuit may already be gone, or the module may fail to dispose; either way there is nothing left to release.
                }
            }
        }
        finally
        {
            _gate.Release();
        }
        // The semaphore is deliberately not disposed: its wait handle is never used, so it holds no unmanaged resources,
        // and disposing it would make a late PrimeAsync throw from WaitAsync or Release.
    }
}
