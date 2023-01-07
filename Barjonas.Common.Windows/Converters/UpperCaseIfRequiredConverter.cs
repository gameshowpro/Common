namespace Barjonas.Common.Converters;

public class UpperCaseIfRequiredConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return UpperCaseIfRequired(value?.ToString());
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
