using System.Globalization;
using MudBlazor.Utilities;

namespace GameshowPro.Common.Blazor;

/// <summary>
/// WCAG 2.x contrast math for the house theme, public so products can guard their own colors in tests.
/// </summary>
public static class GspContrast
{
    /// <summary>The WCAG 2.x AA minimum contrast ratio for normal-size text.</summary>
    public const double NormalText = 4.5;

    /// <summary>The WCAG 2.x AA minimum contrast ratio for large text and non-text parts (icons, borders, focus rings).</summary>
    public const double NonText = 3.0;

    /// <summary>The WCAG relative luminance of an opaque color, 0 (black) to 1 (white).</summary>
    public static double Luminance(MudColor color)
    {
        static double Channel(byte value)
        {
            double c = value / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
        return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
    }

    /// <summary>The WCAG contrast ratio (1 to 21) between two opaque colors; order does not matter.</summary>
    public static double Ratio(MudColor a, MudColor b)
    {
        double la = Luminance(a);
        double lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    /// <summary>The color as lowercase <c>#rrggbb</c>.</summary>
    public static string Hex(MudColor color)
        => string.Create(CultureInfo.InvariantCulture, $"#{color.R:x2}{color.G:x2}{color.B:x2}").ToLowerInvariant();
}
