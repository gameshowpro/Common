// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

public class ToOneBasedConverter : ICommonValueConverter
{
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
