// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts a zero-based integer (0-25) to an uppercase A-Z character; others return empty.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class IntToCharConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt)
        {
            return !valueInt.IsInRange(0, 25) ? string.Empty : (char)(valueInt + 65);
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
