// (C) Barjonas LLC 2018

using System.Globalization;

namespace Barjonas.Common.Converters;

public class AddConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)value + (int)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)value - (int)parameter;
    }
}
