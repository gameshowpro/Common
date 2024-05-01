namespace GameshowPro.Common.Model;

/// <summary>
/// Interface the managed settings for a remote service including high-level properties that can be consumed by the the <see cref="IRemoteService"/>.
/// </summary>
public interface IRemoteServiceSettings
{
    /// <summary>
    /// To be raised whenever <see cref="MonitorUiGroup"/> changes, so that RemoteServiceManager may respond to it.
    /// </summary>
    event EventHandler? MonitorUiGroupChanged;
    /// <summary>
    /// The visual group in which this service should be displayed on the monitoring UI.
    /// </summary>
    int MonitorUiGroup { get; set; }
    /// <summary>
    /// Order in which this service should be shown on the monitoring UI.
    /// </summary>
    int MonitorUiOrder { get; set; }
}
