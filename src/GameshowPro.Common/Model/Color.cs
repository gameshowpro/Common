namespace GameshowPro.Common.Model;

/// <summary>
/// Cross-platform color value used by non-WPF builds.
/// </summary>
public readonly struct Color(byte a, byte r, byte g, byte b) : IEquatable<Color>
{
    public byte A { get; } = a;
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;

    public static Color FromArgb(byte a, byte r, byte g, byte b)
        => new(a, r, g, b);

    public static Color Multiply(Color color, float coefficient)
    {
        float c = float.IsNaN(coefficient) ? 0f : coefficient;
        c = Math.Clamp(c, 0f, 1f);
        return new Color(
            (byte)(color.A * c),
            (byte)(color.R * c),
            (byte)(color.G * c),
            (byte)(color.B * c));
    }

    public static Color operator +(Color left, Color right)
        => new(
            SaturatingAdd(left.A, right.A),
            SaturatingAdd(left.R, right.R),
            SaturatingAdd(left.G, right.G),
            SaturatingAdd(left.B, right.B));

    public bool Equals(Color other)
        => A == other.A && R == other.R && G == other.G && B == other.B;

    public override bool Equals(object? obj)
        => obj is Color other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(A, R, G, B);

    public override string ToString()
        => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    private static byte SaturatingAdd(byte left, byte right)
    {
        int sum = left + right;
        return (byte)(sum > byte.MaxValue ? byte.MaxValue : sum);
    }
}

/// <summary>
/// Predefined colors for non-WPF builds.
/// </summary>
public static class Colors
{
    public static Color Black => Color.FromArgb(byte.MaxValue, 0, 0, 0);
    public static Color White => Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
}
