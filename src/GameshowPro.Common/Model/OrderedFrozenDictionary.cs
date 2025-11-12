namespace GameshowPro.Common.Model;

/// <summary>
/// Factory helpers to create <see cref="OrderedFrozenDictionary{TKey, TValue}"/> instances that preserve original item order alongside a frozen lookup.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public static class OrderedFrozenDictionary
{
    /// <summary>
    /// Creates an <see cref="OrderedFrozenDictionary{TKey, TValue}"/> from key/value pairs.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="source">The key/value pairs used to populate the dictionary.</param>
    /// <param name="comparer">The equality comparer for keys. If null, the default comparer is used.</param>
    /// <returns>An ordered frozen dictionary containing the specified keys and values.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public static OrderedFrozenDictionary<TKey, TValue> ToOrderedFrozenDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
        => new([.. source.Select(kvp => kvp.Value)], source.ToFrozenDictionary(comparer));
    /// <summary>
    /// Creates an <see cref="OrderedFrozenDictionary{TKey, TSource}"/> from a sequence using a key selector.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="comparer">An optional comparer for keys.</param>
    /// <returns>An ordered frozen dictionary with keys selected from the input sequence.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public static OrderedFrozenDictionary<TKey, TSource> ToOrderedFrozenDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
            => new([.. source], source.ToFrozenDictionary(keySelector, comparer));
    /// <summary>
    /// Creates an <see cref="OrderedFrozenDictionary{TKey, TElement}"/> from a sequence using key and element selectors.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type produced by <paramref name="elementSelector"/>.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">Selects a key from each source element.</param>
    /// <param name="elementSelector">Projects the stored value for each source element.</param>
    /// <param name="comparer">An optional comparer for keys.</param>
    /// <returns>An ordered frozen dictionary with keys and values selected from the input sequence.</returns>
    /// <remarks>Docs added by AI.</remarks>
    public static OrderedFrozenDictionary<TKey, TElement> ToOrderedFrozenDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        KeyValuePair<TKey, TElement>[] pairs = [.. source.Select(s => KeyValuePair.Create(keySelector(s), elementSelector(s)))];
        return new([.. pairs.Select(kvp => kvp.Value)], pairs.ToFrozenDictionary(comparer));
    }
}
/// <summary>
/// A wrapper around a frozen dictionary that also keeps a reference to the source array without affecting the original order.
/// </summary>
public class OrderedFrozenDictionary<TKey, TValue> where TKey : notnull
{
    internal OrderedFrozenDictionary(ImmutableArray<TValue> values, FrozenDictionary<TKey, TValue> dictionary) 
    {
        Values = values;
        Dictionary = dictionary;
    }
    /// <summary>
    /// The original values in their natural order.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public ImmutableArray<TValue> Values { get; }
    /// <summary>
    /// The immutable lookup used for fast key-based access.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public FrozenDictionary<TKey, TValue> Dictionary { get; }
}
