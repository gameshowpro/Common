
using MessagePack.Formatters;
using MessagePack;
#nullable enable

namespace Barjonas.Common.Model;

/// <param name="Version">The version of this object, to aid backwards compatability implementation if the type changes.</param>
/// <param name="Index">The zero-based index on the GPI which changed state.</param>
/// <param name="Ordinal">In the case of a faceoff, the zero-based ordinal of this trigger. 0 = 1st.</param>
/// <param name="TimeStamp">In the case of a faceoff, the high-precision interval elapsed since the faceoff started.</param>
/// <param name="IsRising">If this is rising edge (i.e. button is down) then true. If this is a falling edge (e.g. button coming back up) then false.</param>
/// <param name="IsTest">If this edge report is the result of test request then true, otherwise false.</param>
[MessagePackObject, MessagePackFormatter(typeof(EdgeReportFormatter))]
public record EdgeReport(int Version, int Index, int? Ordinal, TimeSpan? TimeStamp, bool IsRising, bool IsTest);
public class EdgeReportFormatter : IMessagePackFormatter<EdgeReport>
{
    internal const byte MessagePackVersion = 1;
    public static readonly EdgeReportFormatter Instance = new();

    private EdgeReportFormatter()
    {
    }

    private const int CurrentFieldCount = 7;
    public EdgeReport Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        int fieldcount = reader.ReadArrayHeader();
        if (fieldcount < CurrentFieldCount)
        {
            throw new MessagePackSerializationException($"Expected at least {CurrentFieldCount} fields. Only found {fieldcount}");
        }
        byte messagePackVersion = reader.ReadByte();
        if (messagePackVersion <= 0)
        {
            throw new MessagePackSerializationException($"Expected at reported message pack version of at least 1");
        }
        int version = reader.ReadInt32();
        int index = reader.ReadInt32();
        int? ordinal = reader.TryReadNil() ? null : reader.ReadInt32();
        TimeSpan? timeStamp = reader.TryReadNil() ? null : new TimeSpan(reader.ReadInt64());
        bool isDown = reader.ReadBoolean();
        bool isTest = reader.ReadBoolean();

        return new(version, index, ordinal, timeStamp, isDown, isTest);
    }

    public void Serialize(ref MessagePackWriter writer, EdgeReport value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(CurrentFieldCount);
        writer.Write(MessagePackVersion);
        writer.Write(value.Version);
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
    }
}
#nullable restore
