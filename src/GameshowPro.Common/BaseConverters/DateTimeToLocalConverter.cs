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
        if (value is DateTime d)
        {
            return d.ToLocalTime();
        }
        return value;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
