// (C) Barjonas LLC 2018

namespace Barjonas.Common.BaseConverters;

public class TimeSpanToSecondsConverter : ICommonValueConverter
{
    private readonly object _unsetValue;
    public TimeSpanToSecondsConverter(object unsetValue)
    {
        _unsetValue = unsetValue;
    }
    public bool IntCeiling { get; set; }
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan span)
        {
            if (IntCeiling)
            {
                return (int)Math.Ceiling(span.TotalSeconds);
            }
            else
            {
                return span.TotalSeconds;
            }
        }
        else
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        else if (typeof(double).IsAssignableFrom(value?.GetType()))
        {
            return TimeSpan.FromSeconds((double)value);
        }
        else if (double.TryParse(value?.ToString(), out var valDouble))
        {
            return TimeSpan.FromSeconds(valDouble);
        }
        else
        {
            return _unsetValue;
        }
    }
}
