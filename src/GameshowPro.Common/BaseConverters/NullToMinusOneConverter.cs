namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Maps null to -1 (or 0 if one-based) for ints/doubles and vice versa on ConvertBack.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class NullToMinusOneConverter : ICommonValueConverter
{
    public bool DisplayIsOneBased { get; set; }
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(double))
        {
            return value == null ? DisplayIsOneBased ? 0d : -1d : (double)value + (DisplayIsOneBased ? 1d : 0d);
        }
        else
        {
            return value == null ? DisplayIsOneBased ? 0 : -1 : (int)value + (DisplayIsOneBased ? 1 : 0);
        }
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            int valInt => DisplayIsOneBased ? valInt == 0 ? null : valInt - 1 : valInt == -1 ? null : valInt,
            double valDouble => DisplayIsOneBased ? valDouble == 0d ? null : valDouble - 1d : valDouble == -1d ? null : valDouble,
            _ => null,
        };
    }
}
