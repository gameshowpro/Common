// (C) Barjonas LLC 2022

namespace GameshowPro.Common.BaseConverters;

public enum StringConverterJoinType
{
    Comma,
    Empty,
    Newline,
    Bullet,
    Tilda
}
public abstract class EnumerableToStringConverter(object doNothing) : ICommonValueConverter
{
    public string NullStringPlaceholder { get; set; } = "NullPlaceholder";
    public string NullNumberPlaceholder { get; set; } = "";
    private static readonly string[] s_joinTypes = [", ", "", "\n", "\n\u2022", "~"];
    public StringConverterJoinType JoinType { get; set; } = StringConverterJoinType.Comma;
    public bool IncludeEmptyItems { get; set; } = false;
    public int IntUiOffset { get; set; } = 1;
    private readonly object _doNothing = doNothing;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        string separator = s_joinTypes[(int)JoinType];
        if (value is IEnumerable<bool> bools)
        {
            return EnumerableToDelimitedString(bools, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<bool?> nbools)
        {
            return EnumerableToDelimitedString(nbools, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<string> strings)
        {
            return EnumerableToDelimitedString(strings, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<string?> nstrings)
        {
            return EnumerableToDelimitedString(nstrings, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<int> ints)
        {
            return EnumerableToDelimitedString(ints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<int?> nints)
        {
            return EnumerableToDelimitedString(nints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<long> longs)
        {
            return EnumerableToDelimitedString(longs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<long?> nlongs)
        {
            return EnumerableToDelimitedString(nlongs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<uint> uints)
        {
            return EnumerableToDelimitedString(uints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<uint?> nuints)
        {
            return EnumerableToDelimitedString(nuints, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<ulong> ulongs)
        {
            return EnumerableToDelimitedString(ulongs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<ulong?> nulongs)
        {
            return EnumerableToDelimitedString(nulongs, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<IEnumerable<int>> intList)
        {
            return EnumerableToDelimitedString(intList, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<float> floats)
        {
            return EnumerableToDelimitedString(floats, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<double> doubles)
        {
            return EnumerableToDelimitedString(doubles, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<float?> nfloats)
        {
            return EnumerableToDelimitedString(nfloats, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
        }
        else if (value is IEnumerable<double?> ndoubles)
        {
            return EnumerableToDelimitedString(ndoubles, separator, IntUiOffset, IncludeEmptyItems, NullNumberPlaceholder, NullStringPlaceholder);
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
                    return ToEnumerableImplementation(DelimitedStringToNonNullableString(valueString, separator, NullStringPlaceholder), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(int))
                {
                    return ToEnumerableImplementation(DelimitedStringToNonNullableType<int>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(uint))
                {
                    return ToEnumerableImplementation(DelimitedStringToNonNullableType<uint>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(long))
                {
                    return ToEnumerableImplementation(DelimitedStringToNonNullableType<long>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(ulong))
                {
                    return ToEnumerableImplementation(DelimitedStringToNonNullableType<ulong>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
                }
                else if (itemType == typeof(bool))
                {
                    return ToEnumerableImplementation(DelimitedStringToNonNullableType<bool>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
                }
            }
            else if (underlyingNullableItemType == typeof(string))
            {
                return ToEnumerableImplementation(DelimitedStringToNullableString(valueString, separator, NullStringPlaceholder), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(int))
            {
                return ToEnumerableImplementation(DelimitedStringToNullableType<int>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(uint))
            {
                return ToEnumerableImplementation(DelimitedStringToNullableType<uint>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(long))
            {
                return ToEnumerableImplementation(DelimitedStringToNullableType<long>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(ulong))
            {
                return ToEnumerableImplementation(DelimitedStringToNullableType<ulong>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
            }

            else if (underlyingNullableItemType == typeof(bool))
            {
                return ToEnumerableImplementation(DelimitedStringToNullableType<bool>(valueString, separator, IntUiOffset), targetType) ?? _doNothing;
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
        else if (targetType.IsAssignableFrom(typeof(FrozenSet<T>)))
        {
            return source?.ToFrozenSet();
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

