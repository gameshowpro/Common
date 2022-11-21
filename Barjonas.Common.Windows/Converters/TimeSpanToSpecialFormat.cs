// (C) Barjonas LLC 2018

using System.Globalization;

namespace Barjonas.Common.Converters;

/// <summary>
/// Convert a TimeSpan to a format which is not possible through simple string.Format() call.
/// ConverterParameter = 0 (or null):
///     For values under one minute, only a number is returned. For other values, mm:ss is returned.
/// ConverterParameter = 1:
///     Total minutes, remaining seconds and milliseconds, e.g. 12:43.4553
/// </summary>
public class TimeSpanToSpecialFormat : IValueConverter
{
    private static readonly TimeSpan s_switchPoint = TimeSpan.FromMinutes(1);
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            if (!int.TryParse(parameter?.ToString(), out int option))
            {
                option = -1;
            }
            switch (option)
            {
                case 1:
                    return $"{(int)ts.TotalMinutes}:{ts.Seconds + ((double)ts.Milliseconds / 1000)}";
                default:
                    if (ts < s_switchPoint)
                    {
                        return ts.ToString(@"%s");
                    }
                    else
                    {
                        return ts.ToString(@"%m\:ss");
                    }
            }
        }
        else
        {
            throw new ArgumentException();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (TimeSpan.TryParse(value.ToString(), out TimeSpan result))
        {
            return result;
        }
        return DependencyProperty.UnsetValue;
    }
}
