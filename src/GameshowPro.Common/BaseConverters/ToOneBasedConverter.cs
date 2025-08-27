// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Offsets zero-based numeric values by +1 for display and parses back to zero-based.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class ToOneBasedConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        return value switch
        {
            byte valueByte => valueByte + 1,
            int valueInt => valueInt + 1,
            _ => null,
        };
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        if (int.TryParse(value?.ToString(), out int intValue))
        {
            return intValue - 1;
        }
        return null;
    }
}
