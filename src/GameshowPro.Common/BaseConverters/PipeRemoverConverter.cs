// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Replaces pipe '|' with space and '~' with a rightwards double arrow.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class PipeRemoverConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.Replace("|", " ").Replace("~", "\u21D2");
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
