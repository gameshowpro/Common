using System.Text.RegularExpressions;

namespace GameshowPro.Common.JsonNet.JsonConverters;
/// <summary>
/// A JsonConverter that serializes <see cref="Type"/> objects with just their assembly-scoped names, but tolerates fully qualified names for deserialization.
/// </summary>
public partial class TypeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
        =>  objectType.IsTypeOrRuntimeType();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is Type type)
        {
            serializer.Serialize(writer, StripDownTypeName(type.AssemblyQualifiedName));
        }
        else
        {
            serializer.Serialize(writer, null);
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (serializer.Deserialize(reader, typeof(string)) is string typeName)
        {
            string name = StripDownTypeName(typeName);
            return Type.GetType(name);
        }
        return null;
    }


    [GeneratedRegex(@",\s*(Version|Culture|PublicKeyToken)=[^\],]*", RegexOptions.Compiled)]
    private static partial Regex StripDownTypeName();

    [return: NotNullIfNotNull(nameof(assemblyQualifiedName))]
    public static string? StripDownTypeName(string? assemblyQualifiedName)
        => assemblyQualifiedName == null ? null : StripDownTypeName().Replace(assemblyQualifiedName, "");
}
