// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Model;

public enum RemoteServiceStates
{
    Disconnected,
    Warning,
    Connected
}

public interface IRemoteService
{
    /// <summary>
    /// Provides a way to express progress to the user in addition to the <see cref="ServiceState"/>.
    /// </summary>
    ServiceState ServiceState { get; }
}
