namespace GameshowPro.Common.Model;

public interface IMdnsServiceSearchProfile
{
    string ServiceType { get; }
    string Protocol { get; }
    bool AllowLocalhost { get; }
}
