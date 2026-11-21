#pragma warning disable IDE0130 // Namespace does not match folder: the clock types are published in GameshowPro.Common.Model; moving them to match the Clocks folder would break consumers.
namespace GameshowPro.Common.Model;
#pragma warning restore IDE0130

/// <summary>A jump of the offset between the monotonic clock and the absolute (wall) clock, as seen by <see cref="AbsoluteClockMap"/>.</summary>
/// <param name="At">The UTC instant, on the new clock, at which the step was detected.</param>
/// <param name="Size">How far the wall clock jumped relative to the monotonic clock. Positive = the wall clock jumped forward.</param>
/// <remarks>Serialized only as part of <see cref="ClockStatus"/>, by <see cref="ClockStatusFormatter"/> (which uses <see cref="ClockStepFormatter"/>).</remarks>
public sealed record ClockStep([property: DataMember] DateTimeOffset At, [property: DataMember] TimeSpan Size);

/// <summary>
/// What a machine can say about the clocks it stamps events with, so that a reader can decide whether two stamps are comparable
/// (see <see cref="OrderingSeparation"/>).
/// </summary>
/// <param name="MonotonicClockId">Names the monotonic ruler (machine and boot). See <see cref="MonotonicClock.Id"/>. Equal ids mean subtractable monotonic readings.</param>
/// <param name="AbsoluteClockId">Names the time source that disciplines the wall clock (for example "ntp:time.example.com"), or this machine's own free-running clock ("system:..."). Equal ids mean the wall clocks follow the same source.</param>
/// <param name="Quality">How well the wall clock is disciplined.</param>
/// <param name="EstimatedError">The time source's own estimate of the maximum error of the wall clock against it, null = unknown. Not a measurement.</param>
/// <param name="LastStep">The last time the wall clock jumped against the monotonic clock, null = none seen since this process started.</param>
/// <param name="UpdatedAt">When this status was last refreshed (UTC), null = unknown.</param>
/// <param name="Detail">A short human-readable reason for the quality, for logs. Null or empty if none.</param>
/// <remarks>
/// Wire evolution is append-only: members are only ever added at the end, and every member after <paramref name="EstimatedError"/> is optional.
/// MessagePack: an array [format version, MonotonicClockId, AbsoluteClockId, Quality, EstimatedError ticks, LastStep, UpdatedAt UTC ticks, Detail];
/// readers need at least 5 elements, supply defaults for missing later ones and skip extra ones. LastStep is [At UTC ticks, Size ticks].
/// System.Text.Json: camelCase properties (members are marked with <see cref="DataMemberAttribute"/> so the repository's opt-in
/// resolver includes them); a missing optional property takes its default and unknown properties are ignored.
/// </remarks>
[MessagePackObject, MessagePackFormatter(typeof(ClockStatusFormatter))]
public sealed record ClockStatus(
    [property: DataMember] string MonotonicClockId,
    [property: DataMember] string AbsoluteClockId,
    [property: DataMember] ClockQuality Quality,
    [property: DataMember] TimeSpan? EstimatedError,
    [property: DataMember] ClockStep? LastStep = null,
    [property: DataMember] DateTimeOffset? UpdatedAt = null,
    [property: DataMember] string? Detail = null);

/// <summary>MessagePack formatter for <see cref="ClockStep"/>: [At UTC ticks, Size ticks, ...]. Extra elements are skipped.</summary>
public sealed class ClockStepFormatter : IMessagePackFormatter<ClockStep?>
{
#pragma warning disable IDE1006 // Naming is required by MessagePack library
    public static readonly ClockStepFormatter Instance = new();
#pragma warning restore IDE1006

    private ClockStepFormatter()
    {
    }

    public ClockStep? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }
        int fieldCount = reader.ReadArrayHeader();
        if (fieldCount < 2)
        {
            throw new MessagePackSerializationException($"Expected at least 2 fields. Only found {fieldCount}");
        }
        DateTimeOffset at = new(reader.ReadInt64(), TimeSpan.Zero);
        TimeSpan size = new(reader.ReadInt64());
        for (int i = 2; i < fieldCount; i++)
        {
            reader.Skip();
        }
        return new ClockStep(at, size);
    }

    public void Serialize(ref MessagePackWriter writer, ClockStep? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }
        writer.WriteArrayHeader(2);
        writer.Write(value.At.UtcTicks);
        writer.Write(value.Size.Ticks);
    }
}

/// <summary>MessagePack formatter for <see cref="ClockStatus"/>. See the remarks on <see cref="ClockStatus"/> for the layout.</summary>
public sealed class ClockStatusFormatter : IMessagePackFormatter<ClockStatus?>
{
    internal const byte FormatVersion = 1;
    private const int MinFieldCount = 5;
    private const int CurrentFieldCount = 8;
#pragma warning disable IDE1006 // Naming is required by MessagePack library
    public static readonly ClockStatusFormatter Instance = new();
#pragma warning restore IDE1006

    private ClockStatusFormatter()
    {
    }

    public ClockStatus? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }
        int fieldCount = reader.ReadArrayHeader();
        if (fieldCount < MinFieldCount)
        {
            throw new MessagePackSerializationException($"Expected at least {MinFieldCount} fields. Only found {fieldCount}");
        }
        byte formatVersion = reader.ReadByte();
        if (formatVersion < 1)
        {
            throw new MessagePackSerializationException("Expected a format version of at least 1");
        }
        string monotonicClockId = reader.ReadString() ?? string.Empty;
        string absoluteClockId = reader.ReadString() ?? string.Empty;
        ClockQuality quality = (ClockQuality)reader.ReadInt32();
        TimeSpan? error = reader.TryReadNil() ? null : new TimeSpan(reader.ReadInt64());
        ClockStep? lastStep = null;
        DateTimeOffset? updatedAt = null;
        string? detail = null;
        if (fieldCount > 5)
        {
            lastStep = ClockStepFormatter.Instance.Deserialize(ref reader, options);
        }
        if (fieldCount > 6)
        {
            updatedAt = reader.TryReadNil() ? null : new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
        }
        if (fieldCount > 7)
        {
            detail = reader.ReadString();
        }
        for (int i = CurrentFieldCount; i < fieldCount; i++)
        {
            reader.Skip();
        }
        return new ClockStatus(monotonicClockId, absoluteClockId, quality, error, lastStep, updatedAt, detail);
    }

    public void Serialize(ref MessagePackWriter writer, ClockStatus? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }
        writer.WriteArrayHeader(CurrentFieldCount);
        writer.Write(FormatVersion);
        writer.Write(value.MonotonicClockId);
        writer.Write(value.AbsoluteClockId);
        writer.Write((int)value.Quality);
        if (value.EstimatedError is TimeSpan error)
        {
            writer.Write(error.Ticks);
        }
        else
        {
            writer.WriteNil();
        }
        ClockStepFormatter.Instance.Serialize(ref writer, value.LastStep, options);
        if (value.UpdatedAt is DateTimeOffset updatedAt)
        {
            writer.Write(updatedAt.UtcTicks);
        }
        else
        {
            writer.WriteNil();
        }
        writer.Write(value.Detail);
    }
}
