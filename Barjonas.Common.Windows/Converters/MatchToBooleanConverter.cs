// (C) Barjonas LLC 2018

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    /// <summary>
    /// Return a visibility or boolean depending on match.
    /// </summary>
    public class MatchToBooleanConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0]?.GetType().IsEnum == true && values[1]?.GetType() == typeof(int))
            {
                return BooleanToType(((int)values[0]).Equals(values[1]), targetType);
            }
            return BooleanToType((values[0] == null && values[1] == null) || values[0]?.Equals(values[1]) == true, targetType);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rawResult = parameter switch
            {
                Type t => t.IsAssignableFrom(value?.GetType()),
                _ => (value == null && parameter == null) || value?.Equals(parameter) == true,
            };
            return BooleanToType(rawResult, targetType);
        }

        protected virtual object BooleanToType(bool value, Type type)
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

        public bool Invert { get; set; }
        public Visibility FalseVisibility { get; set; } = Visibility.Hidden;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing, Binding.DoNothing };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                if (value == null)
                {
                    return parameter == null;
                }
                return value.Equals(parameter);
            }
            return Binding.DoNothing;
        }
    }
}
