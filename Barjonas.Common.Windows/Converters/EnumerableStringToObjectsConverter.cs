// (C) Barjonas LLC 2020

namespace Barjonas.Common.Converters;

public class EnumerableStringToObjectsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}
