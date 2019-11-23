// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    public class ToOneBased : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(byte))
            {
                return (byte)value + 1;
            }
            return (int)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value?.ToString(), out int intval))
            {
                return intval - 1;
            }
            return null;
        }
    }
}
