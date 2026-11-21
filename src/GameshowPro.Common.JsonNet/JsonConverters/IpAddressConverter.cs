#pragma warning disable IDE0130 // Namespace does not match folder: this type is published in this namespace; changing it to match the project root namespace or folder would break consumers and type names stored in settings files.
namespace GameshowPro.Common.JsonNet.JsonConverters;
#pragma warning restore IDE0130

/// <summary>
/// Json.NET converter for <see cref="IPAddress"/> that reads and writes dotted-quad string forms.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public class IPAddressConverter : JsonConverter
{
    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override bool CanConvert(Type objectType)
        => objectType == typeof(IPAddress);

    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        IPAddress? ip = value as IPAddress;
        writer.WriteValue(ip?.ToString());
    }

    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);
        return IPAddress.TryParse(token.Value<string>(), out IPAddress? ipAddress) ? ipAddress : null;
    }
}
