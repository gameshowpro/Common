using MessagePack;
using MessagePack.Formatters;

namespace GameshowPro.Common.MessagePack;

public static class NullableReferenceFormatterExtensions
{
    public static void WriteNullable<T>(this ref MessagePackWriter writer, T? value, MessagePackSerializerOptions options)
        where T : class
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        IMessagePackFormatter<T>? formatter = options.Resolver.GetFormatter<T>();
        if (formatter is null)
        {
            throw new MessagePackSerializationException($"No MessagePack formatter found for {typeof(T)}.");
        }

        formatter.Serialize(ref writer, value, options);
    }

    public static T? ReadNullable<T>(this ref MessagePackReader reader, MessagePackSerializerOptions options)
        where T : class
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        IMessagePackFormatter<T>? formatter = options.Resolver.GetFormatter<T>();
        return formatter is null
            ? throw new MessagePackSerializationException($"No MessagePack formatter found for {typeof(T)}.")
            : formatter.Deserialize(ref reader, options);
    }
}