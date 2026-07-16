using System.Buffers;
using MessagePack;
using MessagePack.Formatters;

namespace GameshowPro.Common.MessagePack;

/// <summary>
/// Serializes <see cref="IPAddress"/> values as raw bytes (4 bytes for IPv4, 16 bytes for IPv6).
/// </summary>
public sealed class IPAddressFormatter : IMessagePackFormatter<IPAddress?>
{
    public static readonly IPAddressFormatter Instance = new();

    private IPAddressFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, IPAddress? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        Span<byte> addressBuffer = stackalloc byte[16];
        if (!value.TryWriteBytes(addressBuffer, out int bytesWritten))
        {
            throw new MessagePackSerializationException("Failed to write IPAddress bytes.");
        }

        writer.Write(addressBuffer[..bytesWritten]);
    }

    public IPAddress? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        ReadOnlySequence<byte>? bytes = reader.ReadBytes();
        if (!bytes.HasValue)
        {
            throw new MessagePackSerializationException("Expected a binary payload for IPAddress.");
        }

        int length = checked((int)bytes.Value.Length);
        if (length is not 4 and not 16)
        {
            throw new MessagePackSerializationException($"Unexpected IPAddress payload length: {length}.");
        }

        Span<byte> addressBuffer = stackalloc byte[16];
        int offset = 0;
        foreach (ReadOnlyMemory<byte> segment in bytes.Value)
        {
            segment.Span.CopyTo(addressBuffer[offset..]);
            offset += segment.Length;
        }

        return offset != length
            ? throw new MessagePackSerializationException("Failed to read IPAddress bytes.")
            : new IPAddress(addressBuffer[..length]);
    }
}