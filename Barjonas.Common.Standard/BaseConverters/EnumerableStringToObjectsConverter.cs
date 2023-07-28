// (C) Barjonas LLC 2020

namespace Barjonas.Common.BaseConverters;

public class EnumerableStringToObjectsConverter : ICommonValueConverter
{
    private readonly object _doNothing;
    public EnumerableStringToObjectsConverter(object doNothing) => _doNothing = doNothing;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        if (value is IEnumerable<string> strings)
        {
            return strings.Select(s => new ViewModel.StringItem(s));
        }
        
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => _doNothing;
}
