
using MessagePack.Formatters;
using MessagePack;


namespace Barjonas.Common.Model;

/// <param name="Version">The version of this object, to aid backwards compatibility implementation if the type changes.</param>
/// <param name="Index">The zero-based index on the GPI which changed state.</param>
/// <param name="TimeStamp">In the case of a faceoff, the high-precision interval elapsed since the faceoff started.</param>
/// <param name="IsLockedOut">If the lockout is beginning then true. If the lockout is ending, then false.</param>
[MessagePackObject, MessagePackFormatter(typeof(LockoutReportFormatter))]
public record LockoutReport(int Version, int Index, TimeSpan TimeStamp, bool IsLockedOut);
public class LockoutReportFormatter : IMessagePackFormatter<LockoutReport>
{
    internal const byte MessagePackVersion = 1;
    public static readonly LockoutReportFormatter Instance = new();

    private LockoutReportFormatter()
    {
    }

    private const int CurrentFieldCount = 5;
    public LockoutReport Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        int fieldCount = reader.ReadArrayHeader();
        if (fieldCount < CurrentFieldCount)
        {
            throw new MessagePackSerializationException($"Expected at least {CurrentFieldCount} fields. Only found {fieldCount}");
        }
        byte messagePackVersion = reader.ReadByte();
        if (messagePackVersion <= 0)
        {
            throw new MessagePackSerializationException($"Expected at reported message pack version of at least 1");
        }
        int version = reader.ReadInt32();
        int index = reader.ReadInt32();
        TimeSpan timeStamp = new(reader.ReadInt64());
        bool isLockedOut = reader.ReadBoolean();

        return new(version, index, timeStamp, isLockedOut);
    }

    public void Serialize(ref MessagePackWriter writer, LockoutReport value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(CurrentFieldCount);
        writer.Write(MessagePackVersion);
        writer.Write(value.Version);
        writer.Write(value.Index);
        writer.Write(value.TimeStamp.Ticks);
        writer.Write(value.IsLockedOut);
    }
}

