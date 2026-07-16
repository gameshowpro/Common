using MessagePack;
using MessagePack.Formatters;

namespace GameshowPro.Common.MessagePack;

public static class NullableFormatterExtensions
{
    public static void WriteNullable<T>(this ref MessagePackWriter writer, T? value, MessagePackSerializerOptions options)
        where T : struct
    {
        if (!value.HasValue)
        {
            writer.WriteNil();
            return;
        }

        IMessagePackFormatter<T>? formatter = options.Resolver.GetFormatter<T>();
        if (formatter is null)
        {
            throw new MessagePackSerializationException($"No MessagePack formatter found for {typeof(T)}.");
        }

        formatter.Serialize(ref writer, value.Value, options);
    }

    public static T? ReadNullable<T>(this ref MessagePackReader reader, MessagePackSerializerOptions options)
        where T : struct
    {
        if (reader.TryReadNil())
        {
            return default;
        }

        IMessagePackFormatter<T>? formatter = options.Resolver.GetFormatter<T>();
        if (formatter is null)
        {
            throw new MessagePackSerializationException($"No MessagePack formatter found for {typeof(T)}.");
        }

        return formatter.Deserialize(ref reader, options);
    }
}