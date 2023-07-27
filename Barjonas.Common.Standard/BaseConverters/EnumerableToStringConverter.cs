// (C) Barjonas LLC 2022

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
    public string NullStringPlaceholder { get; set; } = "NullPlaceholder";
    public string NullNumberPlaceholder { get; set; } = "";
    private static readonly string[] s_joinTypes = new string[] { ", ", "", "\n", "\n\u2022", "~" };
    public StringConverterJoinType JoinType { get; set; } = StringConverterJoinType.Comma;
    public bool IncludeEmptyItems { get; set; } = false;
    public int IntUiOffset { get; set; } = 1;
    private readonly object _doNothing;

    public EnumerableToStringConverter(object doNothing)
    {
        _doNothing = doNothing;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        string separator = s_joinTypes[(int)JoinType];
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
        else if (value is IEnumerable<float> floats)
        {
            return Utils.EnumerableToDelimitedString(floats, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<double> doubles)
        {
            return Utils.EnumerableToDelimitedString(doubles, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<float?> nfloats)
        {
            return Utils.EnumerableToDelimitedString(nfloats, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<double?> ndoubles)
        {
            return Utils.EnumerableToDelimitedString(ndoubles, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else
        {
            return _doNothing;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Type? itemType = null;
        if (targetType.IsGenericType)
        {
            if (targetType.GenericTypeArguments.Length == 1)
            {
                itemType = targetType.GenericTypeArguments[0];
            }
        }
        else if (targetType.IsArray)
        {
            itemType = targetType.GetElementType();
        }
        if (itemType is null)
        {
            return _doNothing;
        }
        Type? underlyingNullableItemType = Nullable.GetUnderlyingType(itemType);
        //To do - find a more graceful way to strongly-type all the most common implementations of IEnumerable<T>, including List, Array, ImmutableList, HashSet
        //Probably use TargetType to break out collection type and item type... make IEnumerable of correct item type, then convert to correct collection type at end using linq.
        string separator = s_joinTypes[(int)JoinType];
        if (value is string valueString)
        {
            if (underlyingNullableItemType == null)
            {
                if (itemType == typeof(string))
                {
                    return ToEnumerableImplementation(Utils.DelimitedStringToNonNullableString(valueString, separator, NullStringPlaceholder), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(int))
                {
                    return ToEnumerableImplementation(Utils.DelimitedStringToNonNullableType<int>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(uint))
                {
                    return ToEnumerableImplementation(Utils.DelimitedStringToNonNullableType<uint>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(long))
                {
                    return ToEnumerableImplementation(Utils.DelimitedStringToNonNullableType<long>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(ulong))
                {
                    return ToEnumerableImplementation(Utils.DelimitedStringToNonNullableType<ulong>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(bool))
                {
                    return ToEnumerableImplementation(Utils.DelimitedStringToNonNullableType<bool>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
                }
            }
            else if (underlyingNullableItemType == typeof(string))
            {
                return ToEnumerableImplementation(Utils.DelimitedStringToNullableString(valueString, separator, NullStringPlaceholder), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(int))
            {
                return ToEnumerableImplementation(Utils.DelimitedStringToNullableType<int>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(uint))
            {
                return ToEnumerableImplementation(Utils.DelimitedStringToNullableType<uint>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(long))
            {
                return ToEnumerableImplementation(Utils.DelimitedStringToNullableType<long>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(ulong))
            {
                return ToEnumerableImplementation(Utils.DelimitedStringToNullableType<ulong>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(bool))
            {
                return ToEnumerableImplementation(Utils.DelimitedStringToNullableType<bool>(valueString, separator, IntUiOffset, NullStringPlaceholder), targetType) ?? _doNothing;
            }
        }
        return _doNothing;
    }

    private static object? ToEnumerableImplementation<T>(IEnumerable<T>? source, Type targetType)
    {
        if (targetType.IsArray)
        {
            return source?.ToArray();
        }
        else if (targetType.IsAssignableFrom(typeof(ImmutableList<T>)))
        {
            return source?.ToImmutableList();
        }
        else if (targetType.IsAssignableFrom(typeof(ImmutableHashSet<T>)))
        {
            return source?.ToImmutableHashSet();
        }
        else if (targetType.IsAssignableFrom(typeof(ObservableCollection<T>)))
        {
            return source is null ? null : new ObservableCollection<T>(source.ToList());
        }
        else if (targetType.IsAssignableFrom(typeof(List<T>)))
        {
            return source?.ToList();
        }
        else if (targetType.IsAssignableFrom(typeof(HashSet<T>)))
        {
            return source?.ToHashSet();
        }
        return source;
    }

}

