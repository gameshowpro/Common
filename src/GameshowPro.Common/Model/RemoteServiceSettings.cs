
namespace GameshowPro.Common.Model;

/// <summary>
/// A WPF-specific implementation for <see cref="IRemoteServiceSettings"/> to be used within remote services' settings classes.
/// </summary>
public class RemoteServiceSettings : ObservableClass, IRemoteServiceSettings
{
    /// <summary>
    /// To be raised whenever <see cref="MonitorUiGroup"/> changes, so that <see cref="IRemoteService"/> may respond to it.
    /// </summary>
    public event EventHandler? MonitorUiGroupChanged;
    /// <summary>
    /// The visual group in which this service should be displayed on the monitoring UI.
    /// </summary>
    private int _monitorUiGroup;
    [DataMember]
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
    private int _monitorUiOrder;
    [DataMember]
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
