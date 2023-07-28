namespace Barjonas.Common.BaseConverters;

public class NullToMinusOneConverter : ICommonValueConverter
{
    public bool DisplayIsOneBased { get; set; }
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(double))
        {
            if (value == null)
            {
                return DisplayIsOneBased ? 0d : -1d;
            }
            else
            {
                return (double)value + (DisplayIsOneBased ? 1d : 0d);
            }
        }
        else
        {
            if (value == null)
            {
                return DisplayIsOneBased ? 0 : -1;
            }
            else
            {
                return (int)value + (DisplayIsOneBased ? 1 : 0);
            }
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case int valInt:
                if (DisplayIsOneBased)
                {
                    return valInt == 0 ? null : valInt - 1;
                }
                else
                {
                    return valInt == -1 ? null : valInt;
                }
            case double valDouble:
                if (DisplayIsOneBased)
                {
                    return valDouble == 0d ? null : valDouble - 1d;
                }
                else
                {
                    return valDouble == -1d ? null : valDouble;
                }
            default:
                return null;
        }
    }
}
