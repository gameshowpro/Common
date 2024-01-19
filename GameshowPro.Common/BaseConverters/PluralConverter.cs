// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

public class PluralConverter : ICommonValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt && parameter is string parameterString)
        {
            return valueInt.PluralIfRequired(parameterString);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
