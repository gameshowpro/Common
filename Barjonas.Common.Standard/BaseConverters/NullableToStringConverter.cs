namespace Barjonas.Common.BaseConverters;

public class NullableToStringConverter : ICommonValueConverter
{
    private const string DefaultNullString = "null";
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value ?? parameter ?? DefaultNullString;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value == (parameter ?? DefaultNullString) ? null : value;
}
