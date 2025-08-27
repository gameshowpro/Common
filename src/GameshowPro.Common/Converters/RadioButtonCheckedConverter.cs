// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Converters;

/// <summary>
/// Converts between a radio button checked state and its bound value or selected key.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class RadioButtonCheckedConverter : IValueConverter, IMultiValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values[0].Equals(values[1]);
    }

    /// <inheritdoc/>
    public object? Convert(object value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        string? p = parameter?.ToString();
        return p is not null && value.Equals(int.Parse(p));
    }

    /// <inheritdoc/>
    public object?[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [(int)value, null];
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter,
        CultureInfo culture)
    {
        string? p = parameter?.ToString();
        return value.Equals(true) && p is not null ? int.Parse(p) : Binding.DoNothing;
    }
}
