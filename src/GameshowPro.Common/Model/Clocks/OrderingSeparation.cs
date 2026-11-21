#pragma warning disable IDE0130 // Namespace does not match folder: the clock types are published in GameshowPro.Common.Model; moving them to match the Clocks folder would break consumers.
namespace GameshowPro.Common.Model;
#pragma warning restore IDE0130

/// <summary>Which rule decided an <see cref="OrderingSeparation"/>.</summary>
public enum OrderingRule
{
    /// <summary>The two stamps cannot be ordered; see <see cref="OrderingSeparation.Reason"/>.</summary>
    NotComparable = 0,

    /// <summary>Same monotonic clock (one machine, one boot): subtract the monotonic readings; only the platforms' ordering resolutions limit the order.</summary>
    SameMonotonicClock = 1,

    /// <summary>Different machines whose wall clocks follow a shared time source: the resolutions plus both clocks' errors.</summary>
    SharedTimeSource = 2,
}

/// <summary>One side of a pair: the ordering resolution of the platform that stamped an event, and the clocks it was stamped with.</summary>
/// <param name="Resolution">The smallest separation at which the platform can order two events; null = unknown.</param>
/// <param name="Clock">The stamping machine's clock status.</param>
public readonly record struct OrderingParticipant(TimeSpan? Resolution, ClockStatus Clock);

/// <summary>
/// The minimum time separation at which two events can be put in order, per pair, and the rule that says so. Events closer than this are tied.
/// </summary>
/// <param name="Separation">The minimum separation, or null if <paramref name="Rule"/> is <see cref="OrderingRule.NotComparable"/>.</param>
/// <param name="Rule">Which rule applied.</param>
/// <param name="Reason">Why the pair is not comparable; null otherwise.</param>
public readonly record struct OrderingSeparation(TimeSpan? Separation, OrderingRule Rule, string? Reason)
{
    /// <summary>True unless <see cref="Rule"/> is <see cref="OrderingRule.NotComparable"/>.</summary>
    public bool IsComparable => Rule != OrderingRule.NotComparable;

    /// <summary>
    /// The per-pair rule, a pure function. In order:
    /// an unknown (or negative) resolution on either side: not comparable.
    /// Same <see cref="ClockStatus.MonotonicClockId"/>: max(resolutionA, resolutionB), <see cref="OrderingRule.SameMonotonicClock"/>.
    /// Otherwise the same <see cref="ClockStatus.AbsoluteClockId"/>, both qualities above <see cref="ClockQuality.None"/> and both
    /// <see cref="ClockStatus.EstimatedError"/> known: max(resolutions) + errorA + errorB, <see cref="OrderingRule.SharedTimeSource"/>.
    /// Otherwise not comparable (different sources, a clock with quality None, or an unknown error).
    /// Empty identifiers never match. Sums saturate at <see cref="TimeSpan.MaxValue"/>.
    /// </summary>
    public static OrderingSeparation Minimum(OrderingParticipant a, OrderingParticipant b)
    {
        ArgumentNullException.ThrowIfNull(a.Clock);
        ArgumentNullException.ThrowIfNull(b.Clock);
        if (a.Resolution is not TimeSpan resolutionA || b.Resolution is not TimeSpan resolutionB)
        {
            return NotComparable("an ordering resolution is unknown");
        }
        if (resolutionA < TimeSpan.Zero || resolutionB < TimeSpan.Zero)
        {
            return NotComparable("an ordering resolution is negative");
        }
        TimeSpan resolution = resolutionA > resolutionB ? resolutionA : resolutionB;

        if (!string.IsNullOrEmpty(a.Clock.MonotonicClockId) && string.Equals(a.Clock.MonotonicClockId, b.Clock.MonotonicClockId, StringComparison.Ordinal))
        {
            return new OrderingSeparation(resolution, OrderingRule.SameMonotonicClock, null);
        }
        if (string.IsNullOrEmpty(a.Clock.AbsoluteClockId) || !string.Equals(a.Clock.AbsoluteClockId, b.Clock.AbsoluteClockId, StringComparison.Ordinal))
        {
            return NotComparable("the clocks follow different time sources");
        }
        if (a.Clock.Quality <= ClockQuality.None || b.Clock.Quality <= ClockQuality.None)
        {
            return NotComparable("a clock has quality None");
        }
        if (a.Clock.EstimatedError is not TimeSpan errorA || b.Clock.EstimatedError is not TimeSpan errorB)
        {
            return NotComparable("a clock's error against the shared time source is unknown");
        }
        if (errorA < TimeSpan.Zero || errorB < TimeSpan.Zero)
        {
            return NotComparable("a clock's error is negative");
        }
        return new OrderingSeparation(Add(Add(resolution, errorA), errorB), OrderingRule.SharedTimeSource, null);
    }

    private static OrderingSeparation NotComparable(string reason) => new(null, OrderingRule.NotComparable, reason);

    private static TimeSpan Add(TimeSpan x, TimeSpan y) => x.Ticks > long.MaxValue - y.Ticks ? TimeSpan.MaxValue : new TimeSpan(x.Ticks + y.Ticks);
}
