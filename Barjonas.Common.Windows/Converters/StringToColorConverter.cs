using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Barjonas.Common.Windows.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color fallback;
            switch (parameter)
            {
                case null:
                    fallback = Colors.White;
                    break;
                case Color c:
                    fallback = c;
                    break;
                default:
                    UtilsWindows.TryStringToColor(parameter.ToString(), out fallback, null);
                    break;
            }
            if (value == null)
            {
                return fallback;
            }
            _ = UtilsWindows.TryStringToColor(value.ToString(), out Color result, fallback);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
