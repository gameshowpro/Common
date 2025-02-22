namespace GameshowPro.Common.Model;

public interface IMdnsInstanceProperties
{
    string InstanceName { get; }
    string ServiceType { get; }
    string Protocol { get; }
    ushort Port { get; }
}
