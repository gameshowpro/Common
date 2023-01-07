namespace Barjonas.Common.Converters;

internal class NullToMinusOneConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value ?? -1;

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int valInt && valInt == -1 ? null : value;
}
