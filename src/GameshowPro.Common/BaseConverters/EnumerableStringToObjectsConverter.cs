﻿// (C) Barjonas LLC 2020

namespace GameshowPro.Common.BaseConverters;

public class EnumerableStringToObjectsConverter(object doNothing) : ICommonValueConverter
{
    private readonly object _doNothing = doNothing;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        if (value is IEnumerable<string> strings)
        {
            return strings.Select(s => new StringItem(s));
        }
        
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => _doNothing;
}
