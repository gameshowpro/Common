// (C) Barjonas LLC 2018

using System;
using System.Globalization;

namespace Barjonas.Common.BaseConverters;

public class EnumToDescriptionConverter : ICommonValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Enum)
        {
            throw new ArgumentException("Value must be an enum");
        }
        if (targetType == typeof(int))
        {
            return (int)value;
        }
        return ((Enum)value).Description();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
