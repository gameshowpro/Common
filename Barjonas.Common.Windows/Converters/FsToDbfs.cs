// (C) Barjonas LLC 2018

using System.Globalization;

namespace Barjonas.Common.Converters;

public class FsToDbfs : IValueConverter
{
    private static double ParameterToMultiplier(object parameter)
    {
        if (parameter == null)
        {
            return 1d;
        }

        double.TryParse(parameter.ToString(), out double mult);
        return mult;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var mult = ParameterToMultiplier(parameter);
        var db = 20 * Math.Log10(System.Convert.ToDouble(value) / mult);
        return db.KeepInRange(-100, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var mult = ParameterToMultiplier(parameter);
        var db = System.Convert.ToDouble(value);
        var fs = mult * Math.Pow(10, db / 20);
        return db <= -100 ? 0 : fs;
    }
}
