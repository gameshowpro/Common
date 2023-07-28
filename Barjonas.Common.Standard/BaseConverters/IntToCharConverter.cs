// (C) Barjonas LLC 2018

namespace Barjonas.Common.BaseConverters;

public class IntToCharConverter : ICommonValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt)
        {
            if (!valueInt.IsInRange(0, 25))
            {
                return string.Empty;
            }
            return (char)(valueInt + 65);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
