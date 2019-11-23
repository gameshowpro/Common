// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    public class RadioButtonCheckedConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0].Equals(values[1]);
        }

        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value.Equals(int.Parse(parameter.ToString()));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { (int)value, null };
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value.Equals(true) ? int.Parse(parameter.ToString()) : Binding.DoNothing;
        }
    }
}
