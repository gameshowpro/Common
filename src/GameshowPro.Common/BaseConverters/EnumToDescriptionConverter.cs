// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts an enum to its DescriptionAttribute text or its integral value when target is int.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class EnumToDescriptionConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum valueEnum)
        {
            if (targetType == typeof(int))
            {
                return (int)value;
            }
            return valueEnum.Description();
        }
        else
        {
            throw new ArgumentException("Value must be an enum");
        }
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
