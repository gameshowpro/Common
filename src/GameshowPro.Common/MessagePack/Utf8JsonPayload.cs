using System.Buffers;
using System.Text.Json;
using System.Text.Json.Nodes;
using MessagePack;

namespace GameshowPro.Common.MessagePack;

internal static class Utf8JsonPayload
{
    public static void WriteElement(ref MessagePackWriter writer, JsonElement element)
    {
        ArrayBufferWriter<byte> buffer = new();
        using (Utf8JsonWriter jsonWriter = new(buffer))
        {
            element.WriteTo(jsonWriter);
        }

        writer.Write(buffer.WrittenSpan);
    }

    public static void WriteDocument(ref MessagePackWriter writer, JsonDocument document)
    {
        WriteElement(ref writer, document.RootElement);
    }

    public static void WriteNode(ref MessagePackWriter writer, JsonNode node)
    {
        ArrayBufferWriter<byte> buffer = new();
        using (Utf8JsonWriter jsonWriter = new(buffer))
        {
            node.WriteTo(jsonWriter);
        }

        writer.Write(buffer.WrittenSpan);
    }

    public static ReadOnlySequence<byte> ReadRequiredBytes(ref MessagePackReader reader, string typeName)
    {
        ReadOnlySequence<byte>? bytes = reader.ReadBytes();
        if (!bytes.HasValue)
        {
            throw new MessagePackSerializationException($"Expected binary payload for {typeName}.");
        }

        return bytes.Value;
    }

    public static JsonDocument ParseDocument(ReadOnlySequence<byte> bytes)
    {
        return JsonDocument.Parse(bytes);
    }

    public static JsonNode ParseNode(ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsSingleSegment)
        {
            JsonNode? node = JsonNode.Parse(bytes.FirstSpan);
            if (node is not null)
            {
                return node;
            }

            throw new MessagePackSerializationException("Invalid JSON payload.");
        }

        int length = checked((int)bytes.Length);
        byte[] rented = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            bytes.CopyTo(rented);
            JsonNode? node = JsonNode.Parse(rented.AsSpan(0, length));
            if (node is not null)
            {
                return node;
            }

            throw new MessagePackSerializationException("Invalid JSON payload.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}