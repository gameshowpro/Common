using MessagePack;
using MessagePack.Formatters;
using System.Reflection;

namespace GameshowPro.Common.MessagePack;

/// <summary>
/// Serializes <see cref="AssemblyName"/> as its display name string.
/// </summary>
public sealed class AssemblyNameFormatter : IMessagePackFormatter<AssemblyName?>
{
    public static readonly AssemblyNameFormatter Instance = new();

    private AssemblyNameFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, AssemblyName? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.Write(value.FullName ?? value.Name);
    }

    public AssemblyName? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        string? displayName = reader.ReadString();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new MessagePackSerializationException("Expected a valid AssemblyName display string.");
        }

        return new AssemblyName(displayName);
    }
}

/// <summary>
/// Serializes exceptions into a safe DTO shape: class name, message, stack trace, and inner exception.
/// </summary>
public sealed class ExceptionFormatter : IMessagePackFormatter<Exception?>
{
    public static readonly ExceptionFormatter Instance = new();

    private ExceptionFormatter()
    {
    }

    public void Serialize(ref MessagePackWriter writer, Exception? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(4);
        writer.Write(value.GetType().FullName ?? value.GetType().Name);
        writer.Write(value.Message);
        writer.Write(value.StackTrace);
        Serialize(ref writer, value.InnerException, options);
    }

    public Exception? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        int count = reader.ReadArrayHeader();
        if (count < 4)
        {
            throw new MessagePackSerializationException($"Expected at least 4 fields for Exception DTO. Found {count}.");
        }

        string className = reader.ReadString() ?? nameof(Exception);
        string message = reader.ReadString() ?? string.Empty;
        string? stackTrace = reader.ReadString();
        Exception? inner = Deserialize(ref reader, options);

        for (int i = 4; i < count; i++)
        {
            reader.Skip();
        }

        return new FormattedException(className, message, stackTrace, inner);
    }

    public sealed class FormattedException : Exception
    {
        public string ClassName { get; }
        public string? CapturedStackTrace { get; }

        internal FormattedException(string className, string message, string? capturedStackTrace, Exception? innerException)
            : base(message, innerException)
        {
            ClassName = className;
            CapturedStackTrace = capturedStackTrace;
        }

        public override string? StackTrace => CapturedStackTrace ?? base.StackTrace;
        public override string ToString() => $"{ClassName}: {Message}";
    }
}