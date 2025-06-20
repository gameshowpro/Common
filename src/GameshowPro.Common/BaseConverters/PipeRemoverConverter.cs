﻿// (C) Barjonas LLC 2018

namespace GameshowPro.Common.BaseConverters;

public class PipeRemoverConverter : ICommonValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.Replace("|", " ").Replace("~", "\u21D2");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
