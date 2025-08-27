// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Converters;

/// <summary>
/// Returns a Visibility or bool depending on whether one value is greater than the other.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class GreaterThanToBooleanConverter : IValueConverter, IMultiValueConverter
{
    /// <summary>When true, greater-than-or-equal counts as true.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool AllowEqual { get; } = false;
    /// <summary>The Visibility value to use when the result is false.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public Visibility FalseVisibility { get; set; } = Visibility.Hidden;
    /// <summary>Compare two bound values and return a boolean/Visibility.</summary>
    /// <param name="values">Array of two values to compare.</param>
    /// <param name="targetType">Target type to return (bool or Visibility).</param>
    /// <param name="parameter">Ignored.</param>
    /// <param name="culture">Culture info.</param>
    /// <remarks>Docs added by AI.</remarks>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 1 && double.TryParse(values[0]?.ToString(), out double a) && double.TryParse(values[1]?.ToString(), out double b))
        {
            return AllowEqual ? BooleanToType(a >= b, targetType) : BooleanToType(a > b, targetType);
        }
        return BooleanToType(false, targetType);
    }

    /// <summary>Compare a value against the supplied parameter.</summary>
    /// <param name="value">The value to check.</param>
    /// <param name="targetType">Target type to return (bool or Visibility).</param>
    /// <param name="parameter">The value to compare against.</param>
    /// <param name="culture">Culture info.</param>
    /// <remarks>Docs added by AI.</remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double dValue && parameter is double dParam)
        {
            return BooleanToType(dValue > dParam, targetType);
        }
        if (value is int iValue && parameter is int iParam)
        {
            return BooleanToType(iValue > iParam, targetType);
        }
        if (value is long lValue && parameter is long lParam)
        {
            return BooleanToType(lValue > lParam, targetType);
        }
        if (value is float fValue && parameter is float fParam)
        {
            return BooleanToType(fValue > fParam, targetType);
        }
        throw new InvalidOperationException("Invalid combination of parameter and value types");
    }

    /// <summary>Convert a boolean result to the requested target type (Visibility or bool).</summary>
    /// <remarks>Docs added by AI.</remarks>
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


    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [Binding.DoNothing, Binding.DoNothing];
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter,
        CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
