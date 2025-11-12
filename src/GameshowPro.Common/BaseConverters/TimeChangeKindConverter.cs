namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts DateTime between kinds; to local on Convert and to UTC on ConvertBack.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class TimeChangeKindConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        //Todo: support other kinds specified by parameter, defaulting to local
        if (value is DateTime valueDateTime)
        {
            return valueDateTime.ToLocalTime();
        }
        else
        {
            return value;
        }
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime valueDateTime)
        {
            return valueDateTime.ToUniversalTime();
        }
        else
        {
            return value;
        }
    }
}
