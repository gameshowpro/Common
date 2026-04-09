namespace GameshowPro.Common.JsonConverters;

internal partial class TypeConverter : JsonConverter<Type?>
{
    public override void Write(Utf8JsonWriter writer, Type? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value == null ? null : TypeAliasRegistry.GetTypeAlias(value));

    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string");
        }
        string? typeName = reader.GetString() ?? throw new JsonException("Could not parse to string");
        return typeName == null ? null : TypeAliasRegistry.ResolveType(typeName);
    }
}
