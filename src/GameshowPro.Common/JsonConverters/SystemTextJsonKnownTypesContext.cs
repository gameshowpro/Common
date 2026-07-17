using System.Text.Json.Serialization.Metadata;

namespace GameshowPro.Common.JsonConverters;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(byte?))]
[JsonSerializable(typeof(char))]
[JsonSerializable(typeof(char?))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTime?))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(decimal?))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(double?))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(float?))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(long?))]
[JsonSerializable(typeof(sbyte))]
[JsonSerializable(typeof(sbyte?))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(short?))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(TimeSpan?))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(uint?))]
[JsonSerializable(typeof(ulong))]
[JsonSerializable(typeof(ulong?))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(ushort?))]
[JsonSerializable(typeof(ImmutableArray<bool>))]
[JsonSerializable(typeof(ImmutableArray<bool>?))]
[JsonSerializable(typeof(ImmutableArray<byte>))]
[JsonSerializable(typeof(ImmutableArray<byte>?))]
[JsonSerializable(typeof(ImmutableArray<double>))]
[JsonSerializable(typeof(ImmutableArray<double>?))]
[JsonSerializable(typeof(ImmutableArray<float>))]
[JsonSerializable(typeof(ImmutableArray<float>?))]
[JsonSerializable(typeof(ImmutableArray<int>))]
[JsonSerializable(typeof(ImmutableArray<int>?))]
[JsonSerializable(typeof(ImmutableArray<string>))]
[JsonSerializable(typeof(ImmutableArray<string>?))]
internal partial class SystemTextJsonKnownTypesContext : JsonSerializerContext
{
}

internal sealed class KnownJsonTypeContextResolver : IJsonTypeInfoResolver
{
    public static KnownJsonTypeContextResolver Instance { get; } = new();

    private readonly Lock _gate = new();
    private readonly Dictionary<JsonSerializerOptions, SystemTextJsonKnownTypesContext> _contexts = [];

    private KnownJsonTypeContextResolver()
    {
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        lock (_gate)
        {
            if (!_contexts.TryGetValue(options, out SystemTextJsonKnownTypesContext? context))
            {
                context = new SystemTextJsonKnownTypesContext(new JsonSerializerOptions(options));
                _contexts.Add(options, context);
            }

            return context.GetTypeInfo(type);
        }
    }

    public static bool TryGetTypeInfo(Type type, JsonSerializerOptions options, [NotNullWhen(true)] out JsonTypeInfo? jsonTypeInfo)
    {
        jsonTypeInfo = Instance.GetTypeInfo(type, options);
        return jsonTypeInfo is not null;
    }
}
