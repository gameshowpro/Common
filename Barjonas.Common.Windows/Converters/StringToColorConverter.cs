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
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Color? fallback = null;
            switch (parameter)
            {
                case null:
                    fallback = Colors.White;
                    break;
                case Color c:
                    fallback = c;
                    break;
                default:
                    string? p = parameter?.ToString();
                    if (p is not null)
                    {
                        UtilsWindows.TryStringToColor(p, out fallback, null);
                    }
                    break;
            }
            if (value == null)
            {
                return fallback;
            }
            string? v = value?.ToString();
            if (v is not null)
            {
                _ = UtilsWindows.TryStringToColor(v, out Color? result, fallback);
                return result;
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
