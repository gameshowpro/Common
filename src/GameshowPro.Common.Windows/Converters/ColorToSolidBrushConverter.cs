namespace GameshowPro.Common.Converters;

public class ColorToSolidBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
