namespace GameshowPro.Common.Model;

public interface IMdnsServiceFinder
{
    public IReadOnlyDictionary<IMdnsServiceSearchProfile, IMdnsMatchedServicesMonitor> Services { get; }
}
