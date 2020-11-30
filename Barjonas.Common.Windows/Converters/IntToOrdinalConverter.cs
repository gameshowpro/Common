// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    public class IntToOrdinalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                if (i < 0)
                {
                    return string.Empty;
                }
                var b = parameter as bool?;
                return i.ToOrdinal(!(b ?? false));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
