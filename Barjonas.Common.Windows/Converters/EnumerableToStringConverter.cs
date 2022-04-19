// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
#nullable enable
namespace Barjonas.Common.Converters;

public enum StringConverterJoinType
{
    Comma,
    Simple,
    Newline,
    Bullet
}
public class EnumerableToStringConverter : IValueConverter
{
    private const string NullStringPlaceholder = "NullPlaceholder";
    private const char NullNumberPlaceholder = '?';
    private static readonly (string,int)[] s_joinTypes = new (string, int)[] { (", ", 2), ("", 0), ("\n", 2), ("\n\u2022", 1) };
    public StringConverterJoinType JoinType { get; set; } = StringConverterJoinType.Comma;
    public bool IncludeEmptyItems { get; set; } = false;
    public int IntUiOffset { get; set; } = 1;

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        
        if (value == null)
        {
            return null;
        }
        (string separator, int trimLength) = s_joinTypes[(int)JoinType];
        if (value is IEnumerable<bool> bools)
        {
            return Utils.EnumerableToDelimitedString(bools, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<bool?> nbools)
        {
            return Utils.EnumerableToDelimitedString(nbools, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<string> strings)
        {
            return Utils.EnumerableToDelimitedString(strings, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<string?> nstrings)
        {
            return Utils.EnumerableToDelimitedString(nstrings, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<int> ints)
        {
            return Utils.EnumerableToDelimitedString(ints, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<int?> nints)
        {
            return Utils.EnumerableToDelimitedString(nints, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<long> longs)
        {
            return Utils.EnumerableToDelimitedString(longs, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<long?> nlongs)
        {
            return Utils.EnumerableToDelimitedString(nlongs, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<uint> uints)
        {
            return Utils.EnumerableToDelimitedString(uints, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<uint?> nuints)
        {
            return Utils.EnumerableToDelimitedString(nuints, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<ulong> ulongs)
        {
            return Utils.EnumerableToDelimitedString(ulongs, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<ulong?> nulongs)
        {
            return Utils.EnumerableToDelimitedString(nulongs, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<IEnumerable<int>> intList)
        {
            return Utils.EnumerableToDelimitedString(intList, separator, trimLength, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else
        {
            return Binding.DoNothing;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        (string seperator, int _) = s_joinTypes[(int)JoinType];
        if (value is string valstr)
        {
            if (targetType.IsAssignableFrom(typeof(IEnumerable<string?>)))
            {
                //return Utils.DelimitedStringToNullableType<string>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<string>)))
            {
                //return Utils.DelimitedStringToNonNullableType<string>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<int>)))
            {
                return Utils.DelimitedStringToNonNullableType<int>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<int?>)))
            {
                return Utils.DelimitedStringToNullableType<int>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<uint>)))
            {
                return Utils.DelimitedStringToNonNullableType<uint>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<uint?>)))
            {
                return Utils.DelimitedStringToNullableType<uint>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<long>)))
            {
                return Utils.DelimitedStringToNonNullableType<long>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<long?>)))
            {
                return Utils.DelimitedStringToNullableType<long>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<ulong>)))
            {
                return Utils.DelimitedStringToNonNullableType<ulong>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<ulong?>)))
            {
                return Utils.DelimitedStringToNullableType<long>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<bool>)))
            {
                return Utils.DelimitedStringToNonNullableType<bool>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
            else if (targetType.IsAssignableFrom(typeof(IEnumerable<bool?>)))
            {
                return Utils.DelimitedStringToNullableType<bool>(valstr, seperator, IntUiOffset, NullStringPlaceholder) ?? Binding.DoNothing;
            }
        }
        return Binding.DoNothing;
    }
}
#nullable restore
