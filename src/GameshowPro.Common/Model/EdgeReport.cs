using MessagePack.Formatters;
using MessagePack;

namespace GameshowPro.Common.Model;

/// <param name="Version">The version of this object, to aid backwards compatibility implementation if the type changes.</param>
/// <param name="Index">The zero-based index on the GPI which changed state.</param>
/// <param name="Ordinal">In the case of a faceoff, the zero-based ordinal of this trigger. 0 = 1st.</param>
/// <param name="TimeStamp">In the case of a faceoff, the high-precision interval elapsed since the faceoff started.</param>
/// <param name="IsRising">If this is rising edge (i.e. button is down) then true. If this is a falling edge (e.g. button coming back up) then false.</param>
/// <param name="IsTest">If this edge report is the result of test request then true, otherwise false.</param>
/// <param name="LockoutTimeRemaining">If this edge was treated as being locked out, the amount of time until the end of that lockout. Otherwise, null.</param>
[MessagePackObject, MessagePackFormatter(typeof(EdgeReportFormatter))]
public record EdgeReport(int Version, int Index, int? Ordinal, TimeSpan? TimeStamp, bool IsRising, bool IsTest, TimeSpan? LockoutTimeRemaining);
public class EdgeReportFormatter : IMessagePackFormatter<EdgeReport?>
{
    internal const byte MessagePackVersion = 1;
#pragma warning disable IDE1006 // Naming is required by MessagePack library
    public static readonly EdgeReportFormatter Instance = new();
#pragma warning restore IDE1006

    private EdgeReportFormatter()
    {
    }

    private const int MinFieldCount = 7;
    private const int CurrentFieldCount = 8;
    public EdgeReport? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        int fieldCount = reader.ReadArrayHeader();
        if (fieldCount < MinFieldCount)
        {
            throw new MessagePackSerializationException($"Expected at least {MinFieldCount} fields. Only found {fieldCount}");
        }
        byte messagePackVersion = reader.ReadByte();
        if (messagePackVersion <= 0)
        {
            throw new MessagePackSerializationException($"Expected at reported message pack version of at least 1");
        }
        int? version = reader.ReadNullableInt32();
        if (version.HasValue)
        {
            int index = reader.ReadInt32();
            int? ordinal = reader.TryReadNil() ? null : reader.ReadInt32();
            TimeSpan? timeStamp = reader.TryReadNil() ? null : new TimeSpan(reader.ReadInt64());
            bool isDown = reader.ReadBoolean();
            bool isTest = reader.ReadBoolean();
            TimeSpan? lockoutTimeRemaining =
                fieldCount > 7 ?
                    reader.TryReadNil() ? null : new TimeSpan(reader.ReadInt64())
                : null;

            return new(version.Value, index, ordinal, timeStamp, isDown, isTest, lockoutTimeRemaining);
        }
        return null;
    }

    public void Serialize(ref MessagePackWriter writer, EdgeReport? value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(CurrentFieldCount);
        writer.Write(MessagePackVersion);
        writer.WriteNullableInt32(value?.Version);
        if (value != null)
        {
            writer.Write(value.Index);
            if (value.Ordinal.HasValue)
            {
                writer.Write(value.Ordinal.Value);
            }
            else
            {
                writer.WriteNil();
            }
            if (value.TimeStamp.HasValue)
            {
                writer.Write(value.TimeStamp.Value.Ticks);
            }
            else
            {
                writer.WriteNil();
            }
            writer.Write(value.IsRising);
            writer.Write(value.IsTest);
            if (value.LockoutTimeRemaining.HasValue)
            {
                writer.Write(value.LockoutTimeRemaining.Value.Ticks);
            }
            else
            {
                writer.WriteNil();
            }
        }
    }
}

