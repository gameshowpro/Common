namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Apply a data-bindable format string to a number.
/// </summary>
public class StringFormatConverter : ICommonMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is not string format)
        {
            return values[0]?.ToString();
        }
        return string.Format($"{{0:{format}}}", values[0]);
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
