using System.Text.Json;
using System.Text.RegularExpressions;

namespace GameshowPro.Common.JsonConverters;

internal partial class TypeConverter : JsonConverter<Type?>
{
    public override void Write(Utf8JsonWriter writer, Type? value, JsonSerializerOptions options)
        => writer.WriteStringValue(IsolateAssemblyAndTypeName(value));

    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string");
        }
        string? typeName = reader.GetString() ?? throw new JsonException("Could not parse to string");
        return typeName == null ? null : Type.GetType(typeName) ?? throw new JsonException("Could not convert to type");
    }


    [GeneratedRegex(@",\s*(Version|Culture|PublicKeyToken)=[^\],]*", RegexOptions.Compiled)]
    private static partial Regex StripDownTypeName();

    [return: NotNullIfNotNull(nameof(assemblyQualifiedName))]
    public static string? StripDownTypeName(string? assemblyQualifiedName)
        => assemblyQualifiedName == null ? null : StripDownTypeName().Replace(assemblyQualifiedName, "");
}
