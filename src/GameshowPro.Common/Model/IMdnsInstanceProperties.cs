namespace GameshowPro.Common.Model;

public interface IMdnsInstanceProperties
{
    string ServiceType { get; }
    string Protocol { get; }
    ushort Port { get; }
}
