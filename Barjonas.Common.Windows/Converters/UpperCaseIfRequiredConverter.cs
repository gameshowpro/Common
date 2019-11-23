using System;
using System.Globalization;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    public class UpperCaseIfRequiredConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Utils.UpperCaseIfRequired(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
