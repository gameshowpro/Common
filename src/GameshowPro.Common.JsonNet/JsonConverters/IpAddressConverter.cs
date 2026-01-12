namespace GameshowPro.Common.JsonNet.JsonConverters;

/// <summary>
/// Json.NET converter for <see cref="IPAddress"/> that reads and writes dotted-quad string forms.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public class IPAddressConverter : JsonConverter
{
    /// <inheritdoc />
    /// <remarks>Docs added by AI.</remarks>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IPAddress);
    }

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
