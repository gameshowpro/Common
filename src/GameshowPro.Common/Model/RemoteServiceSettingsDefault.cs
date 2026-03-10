namespace GameshowPro.Common.Model;
/// <summary>
/// A platform-independent implementation for <see cref="IRemoteServiceSettings"/> to be used within remote services' settings classes.
/// </summary>
[method: JsonConstructor]
public class RemoteServiceSettingsDefault(int? monitorUiGroup, int? monitorUiOrder) : ObservableClass, IRemoteServiceSettings
{
    public RemoteServiceSettingsDefault() : this(null, null)
    {
    }

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
    } = monitorUiGroup ?? 0;

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
    } = monitorUiOrder ?? 0;
}
