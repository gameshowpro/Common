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
        if (reader.TokenType == JsonTokenType.String)
        {
            string? ipString = reader.GetString();
            if (IPAddress.TryParse(ipString, out IPAddress? ip))
            {
                return ip;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IPAddress? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
