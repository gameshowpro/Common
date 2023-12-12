// (C) Barjonas LLC 2018

namespace Barjonas.Common.Converters;

public class RadioButtonCheckedConverter : IValueConverter, IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values[0].Equals(values[1]);
    }

    public object? Convert(object value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        string? p = parameter?.ToString();
        return p is not null && value.Equals(int.Parse(p));
    }

    public object?[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [(int)value, null];
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        CultureInfo culture)
    {
        string? p = parameter?.ToString();
        return value.Equals(true) && p is not null ? int.Parse(p) : Binding.DoNothing;
    }
}
