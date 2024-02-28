namespace GameshowPro.Common.Model;


//
// Summary:
//     Provides a set of initialization methods for instances of the GameshowPro.Common.Model.OrderedFrozenDictionary`2
//     class, which is a wrapper around a <seealso cref="FrozenDictionary"/> that also keeps a reference to the source array without affecting the original order.
public static class OrderedFrozenDictionary
{
    //
    // Summary:
    //     Creates a GameshowPro.Common.Model.OrderedFrozenDictionary`2 with the specified key/value
    //     pairs.
    //
    // Parameters:
    //   source:
    //     The key/value pairs to use to populate the dictionary.
    //
    //   comparer:
    //     The comparer implementation to use to compare keys for equality. If null, System.Collections.Generic.EqualityComparer`1.Default
    //     is used.
    //
    // Type parameters:
    //   TKey:
    //     The type of the keys in the dictionary.
    //
    //   TValue:
    //     The type of the values in the dictionary.
    //
    // Returns:
    //     A GameshowPro.Common.Model.OrderedFrozenDictionary`2 that contains the specified keys
    //     and values.
    public static OrderedFrozenDictionary<TKey, TValue> ToOrderedFrozenDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
        => new(source.Select(kvp => kvp.Value).ToImmutableArray(), source.ToFrozenDictionary(comparer));
    //
    // Summary:
    //     Creates a GameshowPro.Common.Model.OrderedFrozenDictionary`2 from an System.Collections.Generic.IEnumerable`1
    //     according to specified key selector function.
    //
    // Parameters:
    //   source:
    //     An System.Collections.Generic.IEnumerable`1 from which to create a GameshowPro.Common.Model.OrderedFrozenDictionary`2.
    //
    //
    //   keySelector:
    //     A function to extract a key from each element.
    //
    //   comparer:
    //     An System.Collections.Generic.IEqualityComparer`1 to compare keys.
    //
    // Type parameters:
    //   TSource:
    //     The type of the elements of source.
    //
    //   TKey:
    //     The type of the key returned by keySelector.
    //
    // Returns:
    //     A GameshowPro.Common.Model.OrderedFrozenDictionary`2 that contains the keys and values
    //     selected from the input sequence.
    public static OrderedFrozenDictionary<TKey, TSource> ToOrderedFrozenDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
            => new(source.ToImmutableArray(), source.ToFrozenDictionary(keySelector, comparer));
    //
    // Summary:
    //     Creates a GameshowPro.Common.Model.OrderedFrozenDictionary`2 from an System.Collections.Generic.IEnumerable`1
    //     according to specified key selector and element selector functions.
    //
    // Parameters:
    //   source:
    //     An System.Collections.Generic.IEnumerable`1 from which to create a GameshowPro.Common.Model.OrderedFrozenDictionary`2.
    //
    //
    //   keySelector:
    //     A function to extract a key from each element.
    //
    //   elementSelector:
    //     A transform function to produce a result element value from each element.
    //
    //   comparer:
    //     An System.Collections.Generic.IEqualityComparer`1 to compare keys.
    //
    // Type parameters:
    //   TSource:
    //     The type of the elements of source.
    //
    //   TKey:
    //     The type of the key returned by keySelector.
    //
    //   TElement:
    //     The type of the value returned by elementSelector.
    //
    // Returns:
    //     A GameshowPro.Common.Model.OrderedFrozenDictionary`2 that contains the keys and values
    //     selected from the input sequence.
    public static OrderedFrozenDictionary<TKey, TElement> ToOrderedFrozenDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        KeyValuePair<TKey, TElement>[] pairs = source.Select(s => KeyValuePair.Create(keySelector(s), elementSelector(s))).ToArray();
        return new(pairs.Select(kvp => kvp.Value).ToImmutableArray(), pairs.ToFrozenDictionary(comparer));
    }
}
/// <summary>
/// An wrapper around a <seealso cref="FrozenDictionary"/> that also keeps a reference to the source array without affecting the original order.
/// </summary>
public class OrderedFrozenDictionary<TKey, TValue> where TKey : notnull
{
    internal OrderedFrozenDictionary(ImmutableArray<TValue> values, FrozenDictionary<TKey, TValue> dictionary) 
    {
        Values = values;
        Dictionary = dictionary;
    }
    public ImmutableArray<TValue> Values { get; }
    public FrozenDictionary<TKey, TValue> Dictionary { get; }
}
