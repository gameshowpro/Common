namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Uppercases a string when cultural rules require it for display.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class UpperCaseIfRequiredConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return UpperCaseIfRequired(value?.ToString());
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
