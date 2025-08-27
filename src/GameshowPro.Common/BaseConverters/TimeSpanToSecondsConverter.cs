// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts TimeSpan to seconds (double or int via IntCeiling) and parses back from numeric input.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class TimeSpanToSecondsConverter(object unsetValue) : ICommonValueConverter
{
    private readonly object _unsetValue = unsetValue;

    public bool IntCeiling { get; set; }
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
