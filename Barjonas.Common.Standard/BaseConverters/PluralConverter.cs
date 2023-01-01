// (C) Barjonas LLC 2018

namespace Barjonas.Common.BaseConverters;

public class PluralConverter : ICommonValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((int)value).PluralIfRequired(parameter.ToString());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
