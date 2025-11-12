// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Converters;

/// <summary>
/// Converts a TimeSpan or a pair of DateTimes to a sentence expressing elapsed time.
/// If used as <see cref="IMultiValueConverter"/>, values[0] is start, values[1] is end.
/// If used as <see cref="IValueConverter"/>, value is a <see cref="TimeSpan"/>.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class TimeSpanToSentenceConverter : IValueConverter, IMultiValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
        {
            return ts.ToSentence();
        }
        else
        {
            throw new ArgumentException();
        }
    }

    /// <inheritdoc/>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            throw new NotImplementedException();
        }
        if (values[0] is DateTime start && values[1] is DateTime end && start > DateTime.MinValue)
        {
            return Convert(end.ToUniversalTime() - start.ToUniversalTime(), targetType, parameter, culture);
        }
        return "forever";
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (TimeSpan.TryParse(value.ToString(), out TimeSpan result))
        {
            return result;
        }
        return DependencyProperty.UnsetValue;
    }

    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
