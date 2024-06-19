namespace GameshowPro.Common.JsonNet.JsonConverters;

public class IPAddressConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(IPAddress));
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        IPAddress? ip = value as IPAddress;
        writer.WriteValue(ip?.ToString());
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);
        return IPAddress.TryParse(token.Value<string>(), out IPAddress? ipAddress) ? ipAddress : null;
    }
}
