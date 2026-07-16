using MessagePack;
using MessagePack.Formatters;

namespace GameshowPro.Common.MessagePack;

/// <summary>
/// Serializes <see cref="IPEndPoint"/> as [Address, Port].
/// </summary>
public sealed class IPEndPointFormatter : IMessagePackFormatter<IPEndPoint?>
{
    public static readonly IPEndPointFormatter Instance = new();

    private IPEndPointFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, IPEndPoint? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(2);
        IPAddressFormatter.Instance.Serialize(ref writer, value.Address, options);
        writer.Write(value.Port);
    }

    public IPEndPoint? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        int fieldCount = reader.ReadArrayHeader();
        if (fieldCount < 2)
        {
            throw new MessagePackSerializationException($"Expected at least 2 fields for IPEndPoint. Found {fieldCount}.");
        }

        IPAddress? address = IPAddressFormatter.Instance.Deserialize(ref reader, options);
        int port = reader.ReadInt32();

        for (int i = 2; i < fieldCount; i++)
        {
            reader.Skip();
        }

        return address is null
            ? throw new MessagePackSerializationException("Expected non-null IPAddress for IPEndPoint.")
            : new IPEndPoint(address, port);
    }
}