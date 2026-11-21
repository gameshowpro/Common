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
        return reader.ReadBytes() ?? throw new MessagePackSerializationException($"Expected binary payload for {typeName}.");
    }

    public static JsonDocument ParseDocument(ReadOnlySequence<byte> bytes)
    {
        return JsonDocument.Parse(bytes);
    }

    public static JsonNode ParseNode(ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsSingleSegment)
        {
            return JsonNode.Parse(bytes.FirstSpan) ?? throw new MessagePackSerializationException("Invalid JSON payload.");
        }

        int length = checked((int)bytes.Length);
        byte[] rented = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            bytes.CopyTo(rented);
            return JsonNode.Parse(rented.AsSpan(0, length)) ?? throw new MessagePackSerializationException("Invalid JSON payload.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}