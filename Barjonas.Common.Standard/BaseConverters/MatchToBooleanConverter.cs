// (C) Barjonas LLC 2018

using System;
using System.Globalization;

namespace Barjonas.Common.BaseConverters
{
    /// <summary>
    /// Return a visibility or boolean depending on match.
    /// </summary>
    public abstract class MatchToBooleanConverter : ICommonValueConverter, ICommonMultiValueConverter
    {
        private readonly object _doNothing;

        public MatchToBooleanConverter(object doNothing)
        {
            _doNothing = doNothing;
        }

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

        protected abstract object BooleanToType(bool value, Type type);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { _doNothing, _doNothing };
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
            return _doNothing;
        }
    }
}
