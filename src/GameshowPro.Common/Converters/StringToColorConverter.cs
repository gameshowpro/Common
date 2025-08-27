using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace GameshowPro.Common.Converters;

/// <summary>
/// Converts a string (e.g., #RRGGBB or named color) to a Color; supports a fallback Color via parameter.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class StringToColorConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color? fallback = null;
        switch (parameter)
        {
            case null:
                fallback = Colors.White;
                break;
            case Color c:
                fallback = c;
                break;
            default:
                string? p = parameter?.ToString();
                if (p is not null)
                {
                    if (!TryStringToColor(p, out fallback))
                    {
                        fallback = Colors.White;
                    }
                }
                break;
        }
        if (value == null)
        {
            return fallback;
        }
        string? v = value?.ToString();
        if (v is not null)
        {
            if (TryStringToColor(v, out Color? result))
            {
                return result;
            }
            return fallback;
        }
        return null;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
