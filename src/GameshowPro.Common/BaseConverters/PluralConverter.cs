// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Applies pluralization to a word based on an integer count and a format parameter.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class PluralConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int valueInt && parameter is string parameterString ? valueInt.PluralIfRequired(parameterString) : (object?)null;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
