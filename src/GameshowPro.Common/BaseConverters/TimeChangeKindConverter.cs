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
        return value is DateTime valueDateTime ? valueDateTime.ToLocalTime() : value;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DateTime valueDateTime ? valueDateTime.ToUniversalTime() : value;
    }
}
