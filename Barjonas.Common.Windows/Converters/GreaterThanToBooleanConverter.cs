// (C) Barjonas LLC 2018

using System.Globalization;

namespace Barjonas.Common.Converters;

/// <summary>
/// Return a visibility or boolean depending on whether one value is greater than the other.
/// </summary>
public class GreaterThanToBooleanConverter : IValueConverter, IMultiValueConverter
{
    public bool AllowEqual { get; } = false;
    public Visibility FalseVisibility { get; set; } = Visibility.Hidden;
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 1 && double.TryParse(values[0]?.ToString(), out double a) && double.TryParse(values[1]?.ToString(), out double b))
        {
            return AllowEqual ? BooleanToType(a >= b, targetType) : BooleanToType(a > b, targetType);
        }
        return BooleanToType(false, targetType);
    }

    public object Convert(object value, Type targetType, object parameter,
        CultureInfo culture)
    {
        return BooleanToType((double)value > (double)parameter, targetType);
    }

    private object BooleanToType(bool value, Type type)
    {
        if (type != typeof(Visibility))
        {
            return value;
        }
        if (value)
        {
            return Visibility.Visible;
        }
        else
        {
            return FalseVisibility;
        }
    }


    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new object[] { Binding.DoNothing, Binding.DoNothing };
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
