// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Adds an integer parameter to an integer value (and subtracts on ConvertBack).
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class AddConverter(object doNothing) : ICommonValueConverter
{
    private readonly object _doNothing = doNothing;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt && parameter is int valueParameter)
        {
            return valueInt + valueParameter;
        }
        return _doNothing;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt && parameter is int valueParameter)
        {
            return valueInt - valueParameter;
        }
        return _doNothing;
    }
}
