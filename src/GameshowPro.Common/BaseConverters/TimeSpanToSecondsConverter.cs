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
            return IntCeiling ? (int)Math.Ceiling(span.TotalSeconds) : (object)span.TotalSeconds;
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
        else
        {
            return double.TryParse(value?.ToString(), out var valDouble) ? TimeSpan.FromSeconds(valDouble) : _unsetValue;
        }
    }
}
