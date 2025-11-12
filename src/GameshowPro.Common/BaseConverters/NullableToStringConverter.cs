namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts nulls to a display string (default "null") and back to null when that string is seen.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class NullableToStringConverter : ICommonValueConverter
{
    private const string DefaultNullString = "null";
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value ?? parameter ?? DefaultNullString;

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value == (parameter ?? DefaultNullString) ? null : value;
}
