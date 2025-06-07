// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

public class FsToDbfs : ICommonValueConverter
{
    private static double ParameterToMultiplier(object? parameter)
    {
        if (parameter is not null && double.TryParse(parameter.ToString(), out double multiplier))
        {
            return multiplier;
        }
        return 1d;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double multiplier = ParameterToMultiplier(parameter);
        double db = 20 * Math.Log10(System.Convert.ToDouble(value) / multiplier);
        double inRange = db.KeepInRange(-100, 20);
        if (targetType == typeof(string))
        {
            return inRange.ToString("N2");
        }
        return inRange;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double multiplier = ParameterToMultiplier(parameter);
        double db = System.Convert.ToDouble(value);
        double fs = multiplier * Math.Pow(10, db / 20);
        return db <= -100 ? 0 : fs;
    }
}
