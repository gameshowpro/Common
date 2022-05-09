using System;
using System.Globalization;

namespace Barjonas.Common.BaseConverters;

public interface ICommonValueConverter
{
    object Convert(object value, Type targetType, object parameter, CultureInfo culture);
    object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    /// <summary>
    /// Implementors must supply a value for Binding.DoNothing from their native framework.
    /// </summary>
}

public interface ICommonMultiValueConverter
{
    object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
    object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
    /// <summary>
    /// Implementors must supply a value for Binding.DoNothing from their native framework.
    /// </summary>
}

