namespace GameshowPro.Common.Converters;

/// <summary>
/// Converts a Color to a SolidColorBrush.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class ColorToSolidBrushConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => new SolidColorBrush((System.Windows.Media.Color)value);

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
