namespace GameshowPro.Common.Model;

/// <summary>
/// A list of incoming trigger devices of the same type.
/// </summary>
public class IncomingTriggerDevices<TDevice, TSettings>(
    IEnumerable<TSettings> settings,
    Func<TSettings, int, TDevice> deviceFactory)
    : IRemoteServiceCollection where TDevice : IIncomingTriggerDeviceBase, IRemoteService
{
    public ImmutableArray<TDevice> Items { get; } = [.. settings.Select(deviceFactory.Invoke)];

    IEnumerable<IRemoteService> IRemoteServiceCollection.Services => Items.Cast<IRemoteService>();

    /// <summary>
    /// No-op because Adding/removing items at runtime is not currently supported
    /// </summary>
    event NotifyCollectionChangedEventHandler? IRemoteServiceCollection.RemoteServiceCollectionChanged
    {
        add
        {
        }

        remove
        {
        }
    }
}
