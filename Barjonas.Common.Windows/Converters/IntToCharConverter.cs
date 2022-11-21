// (C) Barjonas LLC 2018

using System.Globalization;

namespace Barjonas.Common.Converters;

public class IntToCharConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value.GetType() != typeof(int))
        {
            throw new NotImplementedException();
        }

        var valInt = (int)value;
        if (!valInt.IsInRange(0, 25))
        {
            return string.Empty;
        }

        return (char)((int)value + 65);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
