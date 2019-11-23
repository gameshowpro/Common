// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    /// <summary>
    /// Convert a TimeSpan or pair of datetimes to a sentence expressing how much time has passed.
    /// If using <seealso cref="IMultiValueConverter"/>, first argument is start time, second is end time.
    /// If using <seealso cref="IValueConverter"/>, value is a <seealso cref="TimeSpan"/>.
    /// </summary>
    public class TimeSpanToSentenceConverter : IValueConverter, IMultiValueConverter
    {
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

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                throw new NotImplementedException();
            }
            if (values[0] is DateTime start && values[1] is DateTime end)
            {
                return Convert(end.ToUniversalTime() - start.ToUniversalTime(), targetType, parameter, culture);
            }
            return "forever";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TimeSpan.TryParse(value.ToString(), out TimeSpan result))
            {
                return result;
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
