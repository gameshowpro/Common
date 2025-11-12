// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Converters;

/// <summary>
/// Returns a Visibility or bool depending on whether values match.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class MatchToBooleanConverter : BaseConverters.MatchToBooleanConverter, IValueConverter, IMultiValueConverter
{
    public MatchToBooleanConverter() : base(Binding.DoNothing) { }

    /// <summary>Convert a boolean result to the requested target type (Visibility or bool).</summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="type">The desired target type.</param>
    /// <remarks>Docs added by AI.</remarks>
    protected override object BooleanToType(bool value, Type type)
    {
        if (Invert)
        {
            value ^= true;
        }

        if (type != typeof(Visibility))
        {
            return value;
        }
        if (value)
        {
            return Visibility.Visible;
        }
        else
        {
            return FalseVisibility;
        }
    }

    /// <summary>Invert the match result before converting to the target type.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public bool Invert { get; set; }
    /// <summary>The Visibility value to use when the result is false.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public Visibility FalseVisibility { get; set; } = Visibility.Hidden;
}
