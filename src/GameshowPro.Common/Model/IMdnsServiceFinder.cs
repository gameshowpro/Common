namespace GameshowPro.Common.Model;

public interface IMdnsServiceFinder
{
    IReadOnlyDictionary<IMdnsServiceSearchProfile, IMdnsMatchedServicesMonitor> Services { get; }
}
