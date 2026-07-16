using System.Buffers;
using System.Text.Json.Nodes;
using MessagePack;
using MessagePack.Formatters;

namespace GameshowPro.Common.MessagePack;

/// <summary>
/// Serializes <see cref="JsonNode"/> values as UTF-8 JSON binary payloads.
/// </summary>
public sealed class JsonNodeFormatter : IMessagePackFormatter<JsonNode?>
{
    public static readonly JsonNodeFormatter Instance = new();

    private JsonNodeFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, JsonNode? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        Utf8JsonPayload.WriteNode(ref writer, value);
    }

    public JsonNode? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        ReadOnlySequence<byte> bytes = Utf8JsonPayload.ReadRequiredBytes(ref reader, nameof(JsonNode));
        return Utf8JsonPayload.ParseNode(bytes);
    }
}

/// <summary>
/// Serializes <see cref="JsonObject"/> values as UTF-8 JSON binary payloads.
/// </summary>
public sealed class JsonObjectFormatter : IMessagePackFormatter<JsonObject?>
{
    public static readonly JsonObjectFormatter Instance = new();

    private JsonObjectFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, JsonObject? value, MessagePackSerializerOptions options)
    {
        JsonNodeFormatter.Instance.Serialize(ref writer, value, options);
    }

    public JsonObject? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        ReadOnlySequence<byte> bytes = Utf8JsonPayload.ReadRequiredBytes(ref reader, nameof(JsonObject));
        JsonNode node = Utf8JsonPayload.ParseNode(bytes);
        return node is not JsonObject jsonObject ? throw new MessagePackSerializationException("Expected JSON object payload.") : jsonObject;
    }
}

/// <summary>
/// Serializes <see cref="JsonArray"/> values as UTF-8 JSON binary payloads.
/// </summary>
public sealed class JsonArrayFormatter : IMessagePackFormatter<JsonArray?>
{
    public static readonly JsonArrayFormatter Instance = new();

    private JsonArrayFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, JsonArray? value, MessagePackSerializerOptions options)
    {
        JsonNodeFormatter.Instance.Serialize(ref writer, value, options);
    }

    public JsonArray? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        ReadOnlySequence<byte> bytes = Utf8JsonPayload.ReadRequiredBytes(ref reader, nameof(JsonArray));
        JsonNode node = Utf8JsonPayload.ParseNode(bytes);
        return node is not JsonArray jsonArray ? throw new MessagePackSerializationException("Expected JSON array payload.") : jsonArray;
    }
}

/// <summary>
/// Serializes <see cref="JsonDocument"/> values as UTF-8 JSON binary payloads.
/// </summary>
public sealed class JsonDocumentFormatter : IMessagePackFormatter<JsonDocument?>
{
    public static readonly JsonDocumentFormatter Instance = new();

    private JsonDocumentFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, JsonDocument? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        Utf8JsonPayload.WriteDocument(ref writer, value);
    }

    public JsonDocument? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        ReadOnlySequence<byte> bytes = Utf8JsonPayload.ReadRequiredBytes(ref reader, nameof(JsonDocument));
        return Utf8JsonPayload.ParseDocument(bytes);
    }
}

/// <summary>
/// Serializes <see cref="JsonElement"/> values as UTF-8 JSON binary payloads.
/// </summary>
public sealed class JsonElementFormatter : IMessagePackFormatter<JsonElement?>
{
    public static readonly JsonElementFormatter Instance = new();

    private JsonElementFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, JsonElement? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        Utf8JsonPayload.WriteElement(ref writer, value.Value);
    }

    public JsonElement? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        ReadOnlySequence<byte> bytes = Utf8JsonPayload.ReadRequiredBytes(ref reader, nameof(JsonElement));
        using JsonDocument document = Utf8JsonPayload.ParseDocument(bytes);
        return document.RootElement.Clone();
    }
}