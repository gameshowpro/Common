// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts an integer to its ordinal string (e.g., 1 -> 1st). Optional parameter toggles superscript.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class IntToOrdinalConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }
        else if (value is int i)
        {
            if (i < 0)
            {
                return string.Empty;
            }
            var b = parameter as bool?;
            return i.ToOrdinal(!(b ?? false));
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
