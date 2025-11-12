namespace GameshowPro.Common.JsonNet.JsonConverters;
/// <summary>
/// A JsonConverter that serializes <see cref="Type"/> objects with just their assembly-scoped names, but tolerates fully qualified names for deserialization.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public partial class TypeConverter : JsonConverter
{
    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override bool CanConvert(Type objectType)
        =>  objectType.IsTypeOrRuntimeType();

    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is Type type)
        {
            serializer.Serialize(writer, IsolateAssemblyAndTypeName(type));
        }
        else
        {
            serializer.Serialize(writer, null);
        }
    }

    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (serializer.Deserialize(reader, typeof(string)) is string typeName)
        {
            string name = IsolateAssemblyAndTypeName(typeName);
            return Type.GetType(name);
        }
        return null;
    }
}
