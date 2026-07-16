namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts a DateTime to local time. ConvertBack is not supported.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class DateTimeToLocalConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DateTime d ? d.ToLocalTime() : value;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
