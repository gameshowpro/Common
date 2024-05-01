
namespace GameshowPro.Common.Model;

/// <summary>
/// A WPF-specific implementation for <see cref="IRemoteServiceSettings"/> to be used withing remote services' settings classes.
/// </summary>
[method:JsonConstructor]
public class RemoteServiceSettings(int? monitorUiGroup, int? monitorUiOrder) : NotifyingClass, IRemoteServiceSettings
{
    public RemoteServiceSettings() : this(null, null)
    {
    }

    /// <summary>
    /// To be raised whenever <see cref="MonitorUiGroup"/> changes, so that <see cref="IRemoteService"/> may respond to it.
    /// </summary>
    public event EventHandler? MonitorUiGroupChanged;
    /// <summary>
    /// The visual group in which this service should be displayed on the monitoring UI.
    /// </summary>
    private int _monitorUiGroup = monitorUiGroup ?? 0;
    [JsonProperty]
    public int MonitorUiGroup
    {
        get => _monitorUiGroup;
        set
        {
            if (SetProperty(ref _monitorUiGroup, value))
            {
                MonitorUiGroupChanged?.Invoke(this, new());
            }
        }
    }
    /// <summary>
    /// Order in which this service should be shown on the monitoring UI.
    /// </summary>
    private int _monitorUiOrder = monitorUiOrder ?? 0;
    [JsonProperty]
    public int MonitorUiOrder
    {
        get => _monitorUiOrder;
        set
        {
            if (SetProperty(ref _monitorUiOrder, value))
            {
                MonitorUiGroupChanged?.Invoke(this, new());
            }
        }
    }
}
