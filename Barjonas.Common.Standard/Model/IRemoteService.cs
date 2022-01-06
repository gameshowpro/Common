// (C) Barjonas LLC 2018

using System.ComponentModel;

namespace Barjonas.Common.Model
{
    public enum RemoteServiceStates
    {
        Disconnected,
        Warning,
        Connected
    }

    public interface IRemoteService : INotifyPropertyChanged
    {
        /// <summary>
        /// Provides a way to express progress to the user in addition to the <see cref="ServiceState"/>.
        /// </summary>
        ServiceState ServiceState { get; }
    }
}
