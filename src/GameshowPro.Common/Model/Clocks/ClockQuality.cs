#pragma warning disable IDE0130 // Namespace does not match folder: the clock types are published in GameshowPro.Common.Model; moving them to match the Clocks folder would break consumers.
namespace GameshowPro.Common.Model;
#pragma warning restore IDE0130

/// <summary>How well a wall clock is known to be disciplined to a time source. Ordered: a higher value is a better-disciplined clock.</summary>
/// <remarks>
/// New members may be appended in future versions. Readers must tolerate values they do not know (the MessagePack and
/// System.Text.Json numeric encodings both carry the number; string-based JSON readers will not tolerate an unknown name).
/// </remarks>
public enum ClockQuality
{
    /// <summary>The clock must not be used to compare times with other machines (known broken, or no wall clock). The built-in probes never report this.</summary>
    None = 0,

    /// <summary>The wall clock of unknown or absent discipline: free-running, or an agent exists but does not report that it is synchronized.</summary>
    System = 1,

    /// <summary>Disciplined by NTP (or an NTP-style agent such as chrony or the Windows Time service), within the reported error bound.</summary>
    Ntp = 2,

    /// <summary>Disciplined by PTP with software timestamping.</summary>
    PtpSoftware = 3,

    /// <summary>Disciplined by PTP with hardware timestamping.</summary>
    PtpHardware = 4,
}
