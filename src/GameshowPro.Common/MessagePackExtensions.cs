using MessagePack;
namespace GameshowPro.Common;

public static partial class Utils
{
    /// <summary>
    /// Writes a nullable integer to a MessagePackWriter, writing either an integer or a MessagePackCode.Nil.
    /// </summary>
    /// <param name="writer">Used to write data to a MessagePack format.</param>
    /// <param name="value">Represents an optional integer that may or may not have a value.</param>
    public static void WriteNullableInt32(this ref MessagePackWriter writer, int? value)
    {
        if (value.HasValue)
        {
            writer.Write(value.Value);
        }
        else
        {
            writer.WriteNil();
        }
    }

    /// <summary>
    /// Tries to read an integer value from a MessagePackReader, returning null if the value is MessagePackCode.Nil.
    /// </summary>
    /// <param name="reader">The reader is used to read data from a MessagePack format.</param>
    /// <returns>Returns a nullable integer, which can be an integer value or null.</returns>
    public static int? ReadNullableInt32(this ref MessagePackReader reader)
    {
        if (reader.TryReadNil())
        {
            return null;
        }
        return reader.ReadInt32();
    }

    /// <summary>
    /// Writes a nullable double to a MessagePackWriter, writing either a double or a MessagePackCode.Nil.
    /// </summary>
    /// <param name="writer">Used to write data to a MessagePack format.</param>
    /// <param name="value">Represents an optional double that may or may not have a value.</param>
    public static void WriteNullableDouble(this ref MessagePackWriter writer, double? value)
    {
        if (value.HasValue)
        {
            writer.Write(value.Value);
        }
        else
        {
            writer.WriteNil();
        }
    }

    /// <summary>
    /// Tries to read an double value from a MessagePackReader, returning null if the value is MessagePackCode.Nil.
    /// </summary>
    /// <param name="reader">The reader is used to read data from a MessagePack format.</param>
    /// <returns>Returns a nullable double, which can be an integer value or null.</returns>
    public static double? ReadNullableDouble(this ref MessagePackReader reader)
    {
        if (reader.TryReadNil())
        {
            return null;
        }
        return reader.ReadDouble();
    }

    /// <summary>
    /// Writes a nullable string to a MessagePackWriter, writing either a string or a MessagePackCode.Nil.
    /// </summary>
    /// <param name="writer">Used to write data to a MessagePack format.</param>
    /// <param name="value">Represents an optional string that may or may not have a value.</param>
    public static void WriteNullableString(this ref MessagePackWriter writer, string? value)
    {
        if (value == null)
        {
            writer.WriteNil();
        }
        else
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Tries to read an String value from a MessagePackReader, returning null if the value is MessagePackCode.Nil.
    /// </summary>
    /// <param name="reader">The reader is used to read data from a MessagePack format.</param>
    /// <returns>Returns a nullable string, which can be an string value or null.</returns>
    public static string? ReadNullableString(this ref MessagePackReader reader)
    {
        if (reader.TryReadNil())
        {
            return null;
        }
        return reader.ReadString();
    }
}
