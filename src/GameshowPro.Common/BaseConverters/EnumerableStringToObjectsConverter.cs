// (C) Barjonas LLC 2020

namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Converts IEnumerable&lt;string&gt; to a sequence of StringItem for binding.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class EnumerableStringToObjectsConverter(object doNothing) : ICommonValueConverter
{
    private readonly object _doNothing = doNothing;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        return value is IEnumerable<string> strings ? strings.Select(static s => new StringItem(s)) : (object?)null;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => _doNothing;
}
