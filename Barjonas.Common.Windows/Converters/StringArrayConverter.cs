// (C) Barjonas LLC 2018

using System;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    public class StringArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            Type sourceType = value.GetType();
            if (sourceType == typeof(bool[]))
            {
                var valBool = (bool[])value;
                var sb = new StringBuilder(valBool.Length);
                foreach (var b in valBool)
                {
                    sb.Append(b ? '1' : '0');
                }
                return sb.ToString();
            }
            else if (sourceType == typeof(string[]))
            {
                return string.Join(",", (string[])value);
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,

            System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(bool[]))
            {
                return value.ToString().Select(chr => chr == '1').ToArray();
            }
            else if (targetType == typeof(string[]))
            {
                return value.ToString().Split(',');
            }
            return Binding.DoNothing;
        }
    }
}
