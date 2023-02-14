namespace Barjonas.Common.Converters;

internal class NullableToStringConverter : IValueConverter
{
    private const string DefaultNullString = "null";
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value ?? parameter ?? DefaultNullString;

    public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => value == (parameter ?? DefaultNullString) ? null : value;
}
