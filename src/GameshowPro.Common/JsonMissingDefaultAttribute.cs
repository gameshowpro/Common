namespace GameshowPro.Common;

/// <summary>
/// Specifies a constructor parameter default value to use when the corresponding JSON property is missing.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class JsonMissingDefaultAttribute(object? value) : Attribute
{
    /// <summary>
    /// Gets the value to use when the JSON property is not present.
    /// </summary>
    public object? Value { get; } = value;
}
