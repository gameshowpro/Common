using MessagePack;
using MessagePack.Formatters;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GameshowPro.Common.MessagePack;

/// <summary>
/// Read-only resolver that exposes default BCL formatters for this library.
/// </summary>
public sealed class BclFormatterResolver : IFormatterResolver
{
    public static readonly BclFormatterResolver Instance = new();

    private BclFormatterResolver()
    {
    }

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        return Cache<T>.Formatter;
    }

    private static class Cache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter = (IMessagePackFormatter<T>?)GetFormatterHelper(typeof(T));

        private static object? GetFormatterHelper(Type type)
        {
            if (type == typeof(IPAddress))
            {
                return IPAddressFormatter.Instance;
            }

            if (type == typeof(IPEndPoint))
            {
                return IPEndPointFormatter.Instance;
            }

            if (type == typeof(JsonNode))
            {
                return JsonNodeFormatter.Instance;
            }

            if (type == typeof(JsonObject))
            {
                return JsonObjectFormatter.Instance;
            }

            if (type == typeof(JsonArray))
            {
                return JsonArrayFormatter.Instance;
            }

            if (type == typeof(JsonDocument))
            {
                return JsonDocumentFormatter.Instance;
            }

            if (type == typeof(JsonElement?))
            {
                return JsonElementFormatter.Instance;
            }

            if (type == typeof(AssemblyName))
            {
                return AssemblyNameFormatter.Instance;
            }

            if (type == typeof(Exception))
            {
                return ExceptionFormatter.Instance;
            }

            return null;
        }
    }
}