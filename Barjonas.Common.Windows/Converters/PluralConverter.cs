// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    public class PluralConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter.ToString() + ((int)value == 1 ? "" : "s");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
