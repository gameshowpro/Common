namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Pass-through for int values; returns 0 when the input is not an int.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class ToIntConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valueInt)
        {
            return valueInt;
        }
        return 0;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
