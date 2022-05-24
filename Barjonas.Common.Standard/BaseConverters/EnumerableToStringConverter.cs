// (C) Barjonas LLC 2022
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace Barjonas.Common.BaseConverters;

public enum StringConverterJoinType
{
    Comma,
    Simple,
    Newline,
    Bullet,
    Tilda
}
public abstract class EnumerableToStringConverter : ICommonValueConverter
{
    private const string NullStringPlaceholder = "NullPlaceholder";
    private const string NullNumberPlaceholder = "";
    private static readonly string[] s_joinTypes = new string[] { ", ", "", "\n", "\n\u2022", "~" };
    public StringConverterJoinType JoinType { get; set; } = StringConverterJoinType.Comma;
    public bool IncludeEmptyItems { get; set; } = false;
    public int IntUiOffset { get; set; } = 1;
    private readonly object _doNothing;

    public EnumerableToStringConverter(object doNothing)
    {
        _doNothing = doNothing;
    }

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        
        if (value == null)
        {
            return null;
        }
        string separator= s_joinTypes[(int)JoinType];
        if (value is IEnumerable<bool> bools)
        {
            return Utils.EnumerableToDelimitedString(bools, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<bool?> nbools)
        {
            return Utils.EnumerableToDelimitedString(nbools, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<string> strings)
        {
            return Utils.EnumerableToDelimitedString(strings, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<string?> nstrings)
        {
            return Utils.EnumerableToDelimitedString(nstrings, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<int> ints)
        {
            return Utils.EnumerableToDelimitedString(ints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<int?> nints)
        {
            return Utils.EnumerableToDelimitedString(nints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<long> longs)
        {
            return Utils.EnumerableToDelimitedString(longs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<long?> nlongs)
        {
            return Utils.EnumerableToDelimitedString(nlongs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<uint> uints)
        {
            return Utils.EnumerableToDelimitedString(uints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<uint?> nuints)
        {
            return Utils.EnumerableToDelimitedString(nuints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<ulong> ulongs)
        {
            return Utils.EnumerableToDelimitedString(ulongs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<ulong?> nulongs)
        {
            return Utils.EnumerableToDelimitedString(nulongs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<IEnumerable<int>> intList)
        {
            return Utils.EnumerableToDelimitedString(intList, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else
        {
            return _doNothing;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        //To do - find a more graceful way to strongly-type all the most common implementations of IEnumerable<T>, including List, Array, ImmutableList, HashSet
        //Probably use TargetType to break out collection type and item type... make IEnumerable of correct item type, then convert to correct collection type at end using linq.
        string seperator = s_joinTypes[(int)JoinType];
        if (value is string valstr)
        {
            if (targetType.IsAssignableFrom(typeof(IEnumerable<string?>)))
            {
                //return Utils.DelimitedStringToNullableType<string>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<string>)))
            {
                //return Utils.DelimitedStringToNonNullableType<string>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<string>)))
            {
                return Utils.DelimitedStringToNonNullableString(valstr, seperator, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(string[])))
            {
                return Utils.DelimitedStringToNonNullableString(valstr, seperator, NullStringPlaceholder)?.ToArray() ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<int>)))
            {
                return Utils.DelimitedStringToNonNullableType<int>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(int[])))
            {
                return Utils.DelimitedStringToNonNullableType<int>(valstr, seperator, IntUiOffset, NullStringPlaceholder)?.ToArray() ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<int?>)))
            {
                return Utils.DelimitedStringToNullableType<int>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<uint>)))
            {
                return Utils.DelimitedStringToNonNullableType<uint>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<uint?>)))
            {
                return Utils.DelimitedStringToNullableType<uint>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<long>)))
            {
                return Utils.DelimitedStringToNonNullableType<long>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<long?>)))
            {
                return Utils.DelimitedStringToNullableType<long>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<ulong>)))
            {
                return Utils.DelimitedStringToNonNullableType<ulong>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<ulong?>)))
            {
                return Utils.DelimitedStringToNullableType<ulong>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<bool>)))
            {
                return Utils.DelimitedStringToNonNullableType<bool>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<bool?>)))
            {
                return Utils.DelimitedStringToNullableType<bool>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? _doNothing;
            }
        }
        return _doNothing;
    }
}
#nullable restore
