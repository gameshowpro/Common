namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts a string to uppercase using current culture.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class UpperCaseConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.ToUpper();
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
