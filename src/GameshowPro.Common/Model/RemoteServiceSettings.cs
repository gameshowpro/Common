
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

    [DataMember]
    public int MonitorUiGroup
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                MonitorUiGroupChanged?.Invoke(this, new());
            }
        }
    }

    [DataMember]
    public int MonitorUiOrder
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                MonitorUiGroupChanged?.Invoke(this, new());
            }
        }
    }
}
