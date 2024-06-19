namespace GameshowPro.Common.Model;

public interface IPingHostSettings : INotifyPropertyChanged
{
    string Host { get; }
    IRemoteServiceSettings RemoteServiceSettings { get; }
}
