#pragma warning disable IDE0130 // Namespace does not match folder: this type is published in this namespace; changing it to match the project root namespace or folder would break consumers and type names stored in settings files.
namespace GameshowPro.Common.JsonNet.JsonConverters;
#pragma warning restore IDE0130
/// <summary>
/// A JsonConverter that serializes <see cref="Type"/> objects with just their assembly-scoped names, but tolerates fully qualified names for deserialization.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public partial class TypeConverter : JsonConverter
{
    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override bool CanConvert(Type objectType)
        => objectType.IsTypeOrRuntimeType();

    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value is Type type ? IsolateAssemblyAndTypeName(type) : null);
    }

    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (serializer.Deserialize<string>(reader) is string typeName)
        {
            string name = IsolateAssemblyAndTypeName(typeName);
            return Type.GetType(name);
        }
        return null;
    }
}
