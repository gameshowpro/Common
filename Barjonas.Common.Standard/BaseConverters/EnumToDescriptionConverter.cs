// (C) Barjonas LLC 2018

namespace Barjonas.Common.BaseConverters;

public class EnumToDescriptionConverter : ICommonValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
