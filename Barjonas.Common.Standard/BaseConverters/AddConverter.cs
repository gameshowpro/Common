// (C) Barjonas LLC 2018

namespace Barjonas.Common.BaseConverters;

public class AddConverter : ICommonValueConverter
{
    private readonly object _doNothing;

    public AddConverter(object doNothing)
    {
        _doNothing = doNothing;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt && parameter is int valueParameter)
        {
            return valueInt + valueParameter;
        }
        return _doNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt && parameter is int valueParameter)
        {
            return valueInt - valueParameter;
        }
        return _doNothing;
    }
}
