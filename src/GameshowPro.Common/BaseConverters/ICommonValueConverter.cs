namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Cross-platform abstraction for a value converter.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public interface ICommonValueConverter
{
    /// <summary>Convert a value for binding.</summary>
    /// <param name="value">Source value.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">Culture to use.</param>
    object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);
    /// <summary>Convert a value back for binding.</summary>
    /// <param name="value">Target value.</param>
    /// <param name="targetType">Requested source type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">Culture to use.</param>
    object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
}

/// <summary>
/// Cross-platform abstraction for a multi-value converter.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public interface ICommonMultiValueConverter
{
    /// <summary>Convert values for binding.</summary>
    /// <param name="values">Source values.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">Culture to use.</param>
    object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture);
    /// <summary>Convert values back for binding.</summary>
    /// <param name="value">Target value.</param>
    /// <param name="targetTypes">Requested source types.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">Culture to use.</param>
    object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture);
}

