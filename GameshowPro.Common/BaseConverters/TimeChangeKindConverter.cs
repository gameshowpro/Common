namespace GameshowPro.Common.BaseConverters;

public class TimeChangeKindConverter : ICommonValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        //Todo: support other kinds specified by parameter, defaulting to local
        if (value is DateTime valueDateTime)
        {
            return valueDateTime.ToLocalTime();
        }
        else
        {
            return value;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime valueDateTime)
        {
            return valueDateTime.ToUniversalTime();
        }
        else
        {
            return value;
        }
    }
}
