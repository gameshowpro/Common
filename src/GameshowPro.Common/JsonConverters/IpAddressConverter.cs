namespace GameshowPro.Common.JsonConverters;

/// <summary>
/// System.Text.Json converter for IPAddress that reads/writes dotted-quad string representation.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class IpAddressConverter : JsonConverter<IPAddress?>
{
    /// <inheritdoc/>
    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.String && IPAddress.TryParse(reader.GetString(), out IPAddress? ip) ? ip : null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IPAddress? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}
