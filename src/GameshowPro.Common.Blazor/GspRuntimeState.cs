using GameshowPro.Common.Model;

namespace GameshowPro.Common.Blazor;

/// <summary>
/// The facts only the running app knows, in one injected place that the header, the footer and the About dialog read:
/// the host name, the platform, the connection state and any extra About rows. The app sets them as it learns them; the
/// components re-render. It also carries the request that opens the About dialog, so the header's version pill and the
/// footer need no wiring.
/// </summary>
public sealed class GspRuntimeState
{
    private string? _hostName;
    private string? _platform;
    private RemoteServiceStates? _connection;
    private readonly List<GspAboutRow> _aboutRows = [];

    /// <summary>Raised on the caller's thread after any value changes; components re-render on it.</summary>
    public event Action? Changed;

    /// <summary>Raised by <see cref="RequestAbout"/>; <c>GspAboutDialog</c> opens on it.</summary>
    public event Action? AboutRequested;

    /// <summary>The server's host name, shown in the header (right) and the About table. <c>null</c> hides it.</summary>
    public string? HostName { get => _hostName; set => Set(ref _hostName, value); }

    /// <summary>The platform, for example <c>Windows</c> or <c>ESP32</c>; shown before the version in the header pill and in the About table.</summary>
    public string? Platform { get => _platform; set => Set(ref _platform, value); }

    /// <summary>
    /// The connection to the service, shown as a pill in the header: <see cref="RemoteServiceStates.Connected"/> reads "Connected"
    /// (ok), <see cref="RemoteServiceStates.Warning"/> "Reconnecting" (warn), <see cref="RemoteServiceStates.Disconnected"/>
    /// "Disconnected" (bad). <c>null</c> (not reported) shows no pill.
    /// </summary>
    public RemoteServiceStates? Connection { get => _connection; set => Set(ref _connection, value); }

    /// <summary>Extra rows for the About table, after the standard ones.</summary>
    public IReadOnlyList<GspAboutRow> AboutRows => _aboutRows;

    /// <summary>True once a <c>GspAboutDialog</c> is on the page; the header pill and the footer link only act then.</summary>
    public bool AboutAvailable { get; private set; }

    /// <summary>Adds or replaces an extra About row.</summary>
    public void SetAboutRow(string label, string value)
    {
        int i = _aboutRows.FindIndex(r => r.Label == label);
        GspAboutRow row = new(label, value);
        if (i >= 0)
        {
            if (_aboutRows[i] == row)
            {
                return;
            }
            _aboutRows[i] = row;
        }
        else
        {
            _aboutRows.Add(row);
        }
        Changed?.Invoke();
    }

    /// <summary>Opens the About dialog, if one is on the page.</summary>
    public void RequestAbout() => AboutRequested?.Invoke();

    internal void SetAboutAvailable(bool available)
    {
        if (AboutAvailable != available)
        {
            AboutAvailable = available;
            Changed?.Invoke();
        }
    }

    private void Set<T>(ref T field, T value)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            Changed?.Invoke();
        }
    }
}
