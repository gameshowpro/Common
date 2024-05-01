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
    /// <summary>
    /// If grouping is to be supported, implementors should return an object that provides a grouping index.
    /// </summary>
    IRemoteServiceSettings? Settings { get; }
}

/// <summary>
/// An interface to be implemented by a collection of services implementing <see cref="IRemoteService"/>. This interface provides a way to monitor the collection for changes.
/// </summary>
public interface IRemoteServiceCollection
{
    /// <summary>
    /// Implementors should raise this event when items are added or removed from the collection.
    /// </summary>
    event NotifyCollectionChangedEventHandler? RemoteServiceCollectionChanged;
    IEnumerable<IRemoteService> Services { get; }
}
