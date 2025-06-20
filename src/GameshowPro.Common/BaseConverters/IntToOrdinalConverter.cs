﻿// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

public class IntToOrdinalConverter : ICommonValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return string.Empty;
        }
        else if (value is int i)
        {
            if (i < 0)
            {
                return string.Empty;
            }
            var b = parameter as bool?;
            return i.ToOrdinal(!(b ?? false));
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
