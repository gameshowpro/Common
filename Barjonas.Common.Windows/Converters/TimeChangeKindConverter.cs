using System.Globalization;

namespace Barjonas.Common.Converters;

public class TimeChangeKindConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //Todo: support other kinds specified by parameter, defaulting to local
        return ((DateTime)value).ToLocalTime();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((DateTime)value).ToUniversalTime();
    }
}
