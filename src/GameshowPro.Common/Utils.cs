// (C) Barjonas LLC 2024
namespace GameshowPro.Common;

public static partial class Utils
{
    // Extension members are grouped by receiver type. Extension blocks are used only where the compiler emits a static
    // implementation method with exactly the same signature (including nullability metadata) as the classic form it
    // replaced. Generic members whose type-parameter nullability would change in metadata, and members with decimal
    // default values, remain classic extension methods.

    /// <summary>
    /// Produces a sequence of tuples with elements from the two specified sequences.
    /// If one sequence contains more elements than the other, its missing elements will be filled with the specified value.
    /// </summary>
    /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
    /// <param name="first">The first sequence to merge.</param>
    /// <param name="firstFiller">The filler value to use if the first sequence runs out of elements first.</param>
    /// <param name="second">The second sequence to merge.</param>
    /// <param name="secondFiller">The filler value to use if the second sequence runs out of elements first.</param>
    /// <returns>A sequence of tuples with elements taken from the first add second sequence, in that order.</returns>
    public static IEnumerable<(TFirst First, TSecond Second)> ZipWithFill<TFirst, TSecond>(
        this IEnumerable<TFirst> first,
        TFirst firstFiller,
        IEnumerable<TSecond> second,
        TSecond secondFiller
    )
        => ZipWithFill(first, firstFiller, second, secondFiller, static (e1, e2) => (e1, e2));

    /// <summary>
    /// Produces a sequence of results merged elements from the two specified sequences.
    /// If one sequence contains more elements than the other, its missing elements will be filled with the specified value.
    /// </summary>
    /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements produced by <paramref name="resultSelector"/>.</typeparam>
    /// <param name="first">The first sequence to merge.</param>
    /// <param name="firstFiller">The filler value to use if the first sequence runs out of elements first.</param>
    /// <param name="second">The second sequence to merge.</param>
    /// <param name="secondFiller">The filler value to use if the second sequence runs out of elements first.</param>
    /// <param name="resultSelector">The function used to merge each pair of elements together.</param>
    /// <returns>A sequence of tuples with elements taken from the first add second sequence, in that order.</returns>
    public static IEnumerable<TResult> ZipWithFill<TFirst, TSecond, TResult>(
        this IEnumerable<TFirst> first,
        TFirst firstFiller,
        IEnumerable<TSecond> second,
        TSecond secondFiller,
        Func<TFirst, TSecond, TResult> resultSelector
    )
    {
        using IEnumerator<TFirst> e1 = first.GetEnumerator();
        using IEnumerator<TSecond> e2 = second.GetEnumerator();
        while (e1.MoveNext())
        {
            yield return resultSelector(e1.Current, e2.MoveNext() ? e2.Current : secondFiller);
        }
        while (e2.MoveNext())
        {
            yield return resultSelector(e1.MoveNext() ? e1.Current : firstFiller, e2.Current);
        }
    }

    ///
    /// <summary>Determines whether any element of a sequence satisfies a condition.</summary>
    /// <param name="source">An System.Collections.Generic.IEnumerable`1 whose elements to apply the predicate to.</param>
    /// <param name="predicate">A function to test each element for a condition, including the index of the element.</param>
    /// <typeparam name="TSource">The elements of source.</typeparam>
    /// <returns>
    ///     true if the source sequence is not empty and at least one of its elements passes
    ///     the test in the specified predicate; otherwise, false.
    /// </returns>
    public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
    {
        int index = 0;
        foreach (TSource item in source)
        {
            if (predicate(item, index))
            {
                return true;
            }
            index++;
        }
        return false;
    }

    public static bool TryGetElement<TSource>(this IEnumerable<TSource> source, int index, [MaybeNullWhen(false)] out TSource element)
    {
        Debug.Assert(source is not null);

        if (index >= 0)
        {
            using IEnumerator<TSource> e = source.GetEnumerator();
            while (e.MoveNext())
            {
                if (index == 0)
                {
                    element = e.Current;
                    return true;
                }

                index--;
            }
        }

        element = default;
        return false;
    }

    /// <param name="source">The source sequence</param>
    /// <typeparam name="TSource">The item type from when nullability is to be removed.</typeparam>
    extension<TSource>(IEnumerable<TSource?> source) where TSource : struct
    {
        /// <summary>
        /// Safely filter out null values from a <see cref="IEnumerable{TSource}"/> where <typeparamref name="TSource"/> is a nullable value type, returning a sequence of non-nullable <typeparamref name="TSource"/> values.
        /// </summary>
        public IEnumerable<TSource> WhereNotNull()
            => source.Where(static o => o.HasValue).Select(static o => o!.Value);
    }

    /// <summary>
    /// Safely filter out null values from a <see cref="IEnumerable{TSource}"/> where <typeparamref name="TSource"/> is a nullable reference type, returning a sequence of non-nullable <typeparamref name="TSource"/> values.
    /// </summary>
    /// <typeparam name="TSource">The item type from when nullability is to be removed.</typeparam>
    /// <param name="source">The source sequence</param>
    public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
        => source.Where(static o => o is not null).Select(static o => o!);

    /// <summary>
    /// Perform union with a param array, which may contain 0, 1 or more items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> Union<T>(this IEnumerable<T> source, params T[] items)
        => source is not null ? source.Union((IEnumerable<T>)items) : items;

    /// <summary>
    /// Set the indices of all items in the given list.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list containing the items to be indexed.</param>
    public static void SetIndices<T>(this IEnumerable<T> list) where T : IIndexed
    {
        int i = 0;
        foreach (T item in list)
        {
            item.Index = i;
            i++;
        }
    }

    /// <summary>
    /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>, making the element's positional index available to the action.
    /// </summary>
    /// <param name="list">The source sequence.</param>
    /// <param name="action">The <c>Action&lt;T,int&gt;</c> delegate to perform on each element of the list.</param>
    public static void ForEachWithIndex<T>(this IEnumerable<T> list, Action<T, int> action)
    {
        int index = 0;
        foreach (T item in list)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="list">The source sequence.</param>
    /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the list.</param>
    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        foreach (T item in list)
        {
            action(item);
        }
    }

    /// <param name="source">The source sequence.</param>
    /// <typeparam name="T">The non-nullable value type</typeparam>
    extension<T>(IEnumerable<T> source) where T : struct
    {
        /// <summary>
        /// A custom version of ElementAtOrDefault for value types which will return null if the index is out of range or the source is null.
        /// </summary>
        public T? ElementAtOrNull(int index)
        {
            if (source is null)
            {
                return null;
            }
            else if (source is IReadOnlyList<T> list)
            {
                if (index.IsInRange(0, list.Count - 1))
                {
                    return list[index];
                }
            }
            else if (source is IReadOnlyCollection<T> col)
            {
                if (index.IsInRange(0, col.Count - 1))
                {
                    return col.ElementAt(index);
                }
            }
            else
            {
                if (index.IsInRange(0, source.Count() - 1))
                {
                    return source.ElementAt(index);
                }
            }
            return null;
        }
    }

    /// <param name="source">The source sequence, which may be null.</param>
    /// <typeparam name="T">The item type.</typeparam>
    extension<T>(IEnumerable<T?>? source)
    {
        /// <summary>
        /// Return the lowest non-null value in the given sequence. If the sequence is null or does not contain any non-null value, return null.
        /// </summary>
        public T? MinOrDefault()
        {
            if (source is null)
            {
                return default;
            }

            using IEnumerator<T?> enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return default;
            }

            Comparer<T> comparer = Comparer<T>.Default;
            T? min = enumerator.Current;

            while (enumerator.MoveNext())
            {
                T? current = enumerator.Current;
                if (current is not null && (min is null || comparer.Compare(current, min) < 0))
                {
                    min = current;
                }
            }

            return min;
        }
    }

    /// <summary>
    /// Return the highest non-null value in the given sequence. If the sequence is null or does not contain any non-null value, return null.
    /// </summary>
    public static T? MaxOrDefault<T>(this IEnumerable<T>? source)
    {
        if (source is null)
        {
            return default;
        }

        using IEnumerator<T?> enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return default;
        }

        Comparer<T> comparer = Comparer<T>.Default;
        T? max = enumerator.Current;

        while (enumerator.MoveNext())
        {
            T? current = enumerator.Current;
            if (current is not null && (max is null || comparer.Compare(current, max) > 0))
            {
                max = current;
            }
        }

        return max;
    }

    /// <summary>
    /// Replace any null IEnumerable<typeparamref name="T"/> with an empty Enumerable. Most useful when running a foreach loop on a nullable IEnumerable.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Nullable IEnumerable</param>
    public static IEnumerable<T> NeverNull<T>(this IEnumerable<T>? source)
        => source ?? [];

    /// <param name="source">Nullable IEnumerable</param>
    extension(IEnumerable? source)
    {
        /// <summary>
        /// Replace any null non-generic IEnumerable with an empty Enumerable. Most useful when running a foreach loop on a nullable IEnumerable.
        /// </summary>
        public IEnumerable NeverNull()
            => source ?? Enumerable.Empty<object>();
    }

    /// <param name="source">The source list, which may be null.</param>
    /// <typeparam name="TSource">The non-nullable value type</typeparam>
    extension<TSource>(IReadOnlyList<TSource>? source) where TSource : struct
    {
        /// <summary>
        /// A custom version of FirstOrDefault for value types which will return null if the source is null or empty.
        /// </summary>
        public TSource? FirstOrNull()
            => source is null || source.Count == 0 ? null : source[0];

        /// <summary>
        /// A custom version of FirstOrDefault for value types which will return null if the source is null or empty.
        /// </summary>
        public TSource? LastOrNull()
            => source is null || source.Count == 0 ? null : source[^1];
    }

    public static ImmutableList<T> ImmutableListEnsureCountAndIndices<T>(this IEnumerable<T>? list, int minCount, int maxCount, Func<int, T> factory)
        where T : IIndexed
    {
        ImmutableList<T> built = ImmutableListEnsureCount(list, minCount, maxCount, factory);
        built.SetIndices();
        return built;
    }

    public static ImmutableList<T> ImmutableListEnsureCount<T>(this IEnumerable<T>? list, int minCount, int maxCount, Func<int, T> factory)
    {
        ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
        if (list is not null)
        {
            builder.AddRange(list);
        }
        builder.EnsureListCount(minCount, maxCount, factory);
        return builder.ToImmutable();
    }

    public static ImmutableArray<T> ImmutableArrayEnsureCountAndIndices<T>(this IEnumerable<T>? list, int minCount, int maxCount, Func<int, T> factory)
        where T : IIndexed
    {
        ImmutableArray<T> built = ImmutableArrayEnsureCount(list, minCount, maxCount, factory);
        built.SetIndices();
        return built;
    }

    public static ImmutableArray<T> ImmutableArrayEnsureCount<T>(this IEnumerable<T>? list, int minCount, int maxCount, Func<int, T> factory)
    {
        ImmutableArray<T>.Builder builder = ImmutableArray.CreateBuilder<T>();
        if (list is not null && !(list is ImmutableArray<T> array && array.IsDefault))
        {
            builder.AddRange(list);
        }
        builder.EnsureListCount(minCount, maxCount, factory);
        return builder.ToImmutable();
    }

    /// <summary>
    /// Find the index of the first item in the given ImmutableArray which matches the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="array">The array to be searched.</param>
    /// <param name="match">The predicate to match again.</param>
    /// <returns>The index of the first item that matches the predicate.</returns>
    public static int FindIndex<T>(this ImmutableArray<T> array, Predicate<T> match)
        => FindIndex(array, 0, array.Length, match);

    /// <summary>
    /// Find the index of the first item in the given ImmutableArray which matches the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="array">The array to be searched.</param>
    /// <param name="startIndex">The index to start searching from.</param>
    /// <param name="match">The predicate to match again.</param>
    /// <returns>The index of the first item that matches the predicate.</returns>
    public static int FindIndex<T>(this ImmutableArray<T> array, int startIndex, Predicate<T> match)
        => FindIndex(array, startIndex, array.Length - startIndex, match);

    /// <summary>
    /// Find the index of the first item in the given ImmutableArray which matches the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="array">The array to be searched.</param>
    /// <param name="startIndex">The index to start searching from.</param>
    /// <param name="count">The number of items to search.</param>
    /// <param name="match">The predicate to match again.</param>
    /// <returns>The index of the first item that matches the predicate.</returns>
    public static int FindIndex<T>(this ImmutableArray<T> array, int startIndex, int count, Predicate<T> match)
    {
        int endIndex = startIndex + count;
        for (int i = startIndex; i < endIndex; i++)
        {
            if (match(array[i]))
            {
                return i;
            }
        }
        return -1;
    }

    // Adds the elements of the given collection to the end of this list unless they are null. If
    // required, the capacity of the list is increased to twice the previous
    // capacity or the new size, whichever is larger.
    public static void AddRange<T>(this List<T> list, IEnumerable<T>? collection)
    {
        if (collection is not null)
        {
            list.AddRange(collection);
        }
    }

    /// <summary>
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this List<T> list, int minCount, Func<int, T> factory)
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, int.MaxValue, factory);
    }

    /// <summary>
    /// Ensure the given generic list is sized within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this List<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list is sized within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this IList<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        ArgumentNullException.ThrowIfNull(list);
        while (!list.Count.IsInRange(minCount, maxCount, true))
        {
            if (list.Count > minCount)
            {
                list.RemoveAt(list.Count - 1);
            }
            else
            {
                list.Add(factory(list.Count));
            }
        }
    }

    /// <summary>
    /// Ensure the given generic list contains at least <paramref name="minCount"/> entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this IList<T> list, int minCount, Func<int, T> factory)
        => EnsureListCount(list, minCount, int.MaxValue, factory);

    public static bool TryGetElement<TSource>(this IList<TSource> source, int index, [MaybeNullWhen(false)] out TSource element)
    {
        Debug.Assert(source is not null);

        if (index >= 0 && source.Count > index)
        {
            element = source[index];
            return true;
        }
        element = default;
        return false;
    }

    /// <param name="list">The list containing the item.</param>
    /// <typeparam name="T">The type of the items.</typeparam>
    extension<T>(IList<T>? list) where T : IIndexed
    {
        /// <summary>
        /// Nudge a given item in a given list by a given number of positions.
        /// </summary>
        public void Nudge(T? item, int vector)
        {
            if (list is null || item is null)
            {
                return;
            }
            int newIndex = item.Index + vector;
            if (newIndex.IsInRange(0, list.Count, false))
            {
                list.RemoveAt(item.Index);
                list.Insert(item.Index + vector, item);
                SetIndices(list);
            }
        }
    }

    public static void Nudge<T>(this T item, int vector) where T : IIndexed, IListChild<T>
        => Nudge(item?.Parent, item, vector);

    /// <summary>
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this ObservableCollection<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list contains at least <paramref name="minCount"/> and at most <paramref name="maxCount"/> entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this ObservableCollectionEx<T> list, int minCount, int maxCount, Func<int, T> factory) where T : INotifyPropertyChanged
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given binding list is sized within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this BindingList<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given immutable list builder is sized within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the immutable list builder.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this ImmutableList<T>.Builder list, int minCount, int maxCount, Func<int, T> factory)
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given immutable list is sized within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the immutable list.</typeparam>
    /// <param name="list">The immutable list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static ImmutableList<T> EnsureListCount<T>(this ImmutableList<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        ArgumentNullException.ThrowIfNull(list);
        if (list.Count.IsInRange(minCount, maxCount))
        {
            return list;
        }
        ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
        builder.AddRange(list);
        while (!builder.Count.IsInRange(minCount, maxCount, true))
        {
            if (builder.Count > minCount)
            {
                builder.RemoveAt(builder.Count - 1);
            }
            else
            {
                builder.Add(factory(builder.Count));
            }
        }
        return builder.ToImmutable();
    }

    /// <summary>
    /// Alternative to <c>IDictionary&lt;TKey,TValue&gt;.TryGetValue</c> better suited to inlining when key is value type.
    /// </summary>
    public static TValue? TryGetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey? key)
        where TKey : struct
        => key.HasValue && dictionary?.TryGetValue(key.Value, out TValue? value) == true ? value : default;

    /// <summary>
    /// Alternative to <c>IDictionary&lt;TKey,TValue&gt;.TryGetValue</c> better suited to inlining when key is reference type.
    /// </summary>
    public static TValue? TryGetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey? key)
        where TKey : class
        => key is not null && dictionary?.TryGetValue(key, out TValue? value) == true ? value : default;

    /// <param name="input">The number to be compared or kept in bounds.</param>
    extension(int input)
    {
        /// <summary>
        /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the maximum bound can also result in a true result.
        /// </summary>
        ///  <param name="min">The minimum value of the number.</param>
        ///  <param name="max">The maximum value of the number (inclusive or exclusive, depending on <paramref name="maxIsInclusive"/>).</param>
        ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
        public bool IsInRange(int min, int max, bool maxIsInclusive = true)
            => maxIsInclusive ? input <= max && input >= min : input < max && input >= min;

        /// <summary>
        /// Keeps an Int32 within the given bounds, clipping it if it falls outside.
        /// </summary>
        /// <remarks>
        /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
        /// </remarks>
        ///  <param name="floor">The lowest acceptable value.</param>
        ///  <param name="ceiling">The highest acceptable value.</param>
        public int KeepInRange(int floor = int.MinValue, int ceiling = int.MaxValue)
        {
            if (input > ceiling)
            {
                return ceiling;
            }

            return input < floor ? floor : input;
        }
    }

    /// <param name="input">The number to be compared.</param>
    extension(int? input)
    {
        public bool IsInRange(int min, int max, bool maxIsInclusive = true)
            => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);
    }

    /// <param name="value">The value to be converted.</param>
    extension(int? value)
    {
        /// <summary>
        /// Convert an integer to a string representing the corresponding ordinal, e.g. 1st, 2nd, 3rd etc.
        /// </summary>
        /// <param name="fromZeroBased">If true,0 is 1st otherwise 1 is 1st.</param>
        public string ToOrdinal(bool fromZeroBased = false)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }
            int oneBased = value.Value + (fromZeroBased ? 1 : 0);
            // 11th, 12th and 13th (and 111th, 212th...) are the exceptions to the last-digit rule.
            int lastTwo = Math.Abs(oneBased % 100);
            string suffix = lastTwo is >= 11 and <= 13
                ? "th"
                : (lastTwo % 10) switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th",
                };
            return oneBased.ToString(CultureInfo.InvariantCulture) + suffix;
        }
    }

    /// <param name="value">The value to be converted.</param>
    extension(int value)
    {
        public string ToOrdinal(bool fromZeroBased = false)
            => ToOrdinal((int?)value, fromZeroBased);
    }

    /// <param name="number">The number to be described.</param>
    extension(int number)
    {
        public string PluralOrSingular(string singularNoun, string pluralNoun)
            => $"{number} {(number == 1 ? singularNoun : pluralNoun)}";

        public string PluralIfRequired(string singularNoun, string pluralSuffix = "s")
            => $"{number} {singularNoun}{(number == 1 ? "" : pluralSuffix)}";

        public string ToWords()
        {
            if (number == 0)
            {
                return "zero";
            }

            if (number < 0)
            {
                return "minus " + ToWords(Math.Abs(number));
            }

            StringBuilder words = new();

            if ((number / 1000000) > 0)
            {
                _ = words.Append(ToWords(number / 1000000) + " million ");
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                _ = words.Append(ToWords(number / 1000) + " thousand ");
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                _ = words.Append(ToWords(number / 100) + " hundred ");
                number %= 100;
            }

            if (number > 0)
            {
                if (words.Length > 0)
                {
                    _ = words.Append("and ");
                }

                if (number < 20)
                {
                    _ = words.Append(s_unitsMap[number]);
                }
                else
                {
                    _ = words.Append(s_tensMap[number / 10]);
                    if ((number % 10) > 0)
                    {
                        _ = words.Append("-" + s_unitsMap[number % 10]);
                    }
                }
            }

            return words.ToString();
        }
    }

    /// <param name="input">The number to be compared.</param>
    extension(byte input)
    {
        /// <summary>
        /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the maximum bound can also result in a true result.
        /// </summary>
        ///  <param name="min">The minimum value of the number.</param>
        ///  <param name="max">The maximum value of the number (inclusive or exclusive, depending on <paramref name="maxIsInclusive"/>).</param>
        ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
        public bool IsInRange(byte min, byte max, bool maxIsInclusive = true)
            => maxIsInclusive ? input <= max && input >= min : input < max && input >= min;
    }

    /// <param name="input">The number to be compared.</param>
    extension(byte? input)
    {
        public bool IsInRange(byte min, byte max, bool maxIsInclusive = true)
            => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);
    }

    /// <param name="input">The number to be compared or kept in bounds.</param>
    extension(long input)
    {
        /// <summary>
        /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the maximum bound can also result in a true result.
        /// </summary>
        ///  <param name="min">The minimum value of the number.</param>
        ///  <param name="max">The maximum value of the number (inclusive or exclusive, depending on <paramref name="maxIsInclusive"/>).</param>
        ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
        public bool IsInRange(long min, long max, bool maxIsInclusive = true)
            => maxIsInclusive ? input <= max && input >= min : input < max && input >= min;

        /// <summary>
        /// Keeps an Int64 within the given bounds, clipping it if it falls outside.
        /// </summary>
        /// <remarks>
        /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
        /// </remarks>
        ///  <param name="floor">The lowest acceptable value.</param>
        ///  <param name="ceiling">The highest acceptable value.</param>
        public long KeepInRange(long floor = long.MinValue, long ceiling = long.MaxValue)
        {
            if (input > ceiling)
            {
                return ceiling;
            }

            return input < floor ? floor : input;
        }
    }

    /// <param name="input">The number to be compared.</param>
    extension(long? input)
    {
        public bool IsInRange(long min, long max, bool maxIsInclusive = true)
            => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);
    }

    /// <param name="number">The number to be narrowed.</param>
    extension(long number)
    {
        /// <summary>
        /// Narrow a <see cref="long"/> to an int, clamping to <see cref="int.MaxValue"/> or <see cref="int.MinValue"/> if it's out of range.
        /// </summary>
        public int NarrowToIntClamped()
            => (int)KeepInRange(number, int.MinValue, int.MaxValue);
    }

    /// <param name="number">The number to operate upon.</param>
    extension(ulong number)
    {
        /// <summary>
        /// Narrow a <see cref="ulong"/> to an int, clamping to <see cref="int.MaxValue"/> if it's out of range.
        /// </summary>
        public int NarrowToIntClamped()
            => number > int.MaxValue ? int.MaxValue : (int)number;

        /// <summary>
        /// Calculate the factorial of the given number.
        /// </summary>
        /// <remarks>Factorials are often used to calculate the possible distinct sequences of distinct objects. <paramref name="number"/> is, for example, the number of distinct objects.</remarks>
        public ulong Factorial()
        {
            if (number < 2)
            {
                return 1;
            }

            checked
            {
                ulong factorial = number;
                for (ulong i = 2; i < number; i++)
                {
                    factorial *= i;
                }
                return factorial;
            }
        }
    }

    /// <param name="number">The number to be narrowed.</param>
    extension(uint number)
    {
        /// <summary>
        /// Narrow a <see cref="uint"/> to an int, clamping to <see cref="int.MaxValue"/> if it's out of range.
        /// </summary>
        public int NarrowToIntClamped()
            => number > int.MaxValue ? int.MaxValue : (int)number;
    }

    /// <param name="input">The number to be compared or kept in bounds.</param>
    extension(float input)
    {
        /// <summary>
        /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the maximum bound can also result in a true result.
        /// </summary>
        ///  <param name="min">The minimum value of the number.</param>
        ///  <param name="max">The maximum value of the number (inclusive or exclusive, depending on <paramref name="maxIsInclusive"/>).</param>
        ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
        public bool IsInRange(float min, float max, bool maxIsInclusive = true)
            => maxIsInclusive ? input <= max && input >= min : input < max && input >= min;

        /// <summary>
        /// Keeps a Single within the given bounds, clipping it if it falls outside.
        /// </summary>
        /// <remarks>
        /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
        /// </remarks>
        ///  <param name="floor">The lowest acceptable value.</param>
        ///  <param name="ceiling">The highest acceptable value.</param>
        public float KeepInRange(float floor = float.MinValue, float ceiling = float.MaxValue)
        {
            if (input > ceiling)
            {
                return ceiling;
            }

            return input < floor || float.IsNaN(input) ? floor : input;
        }
    }

    /// <param name="input">The number to be compared.</param>
    extension(float? input)
    {
        public bool IsInRange(float min, float max, bool maxIsInclusive = true)
            => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);
    }

    /// <param name="input">The number to be compared, kept in bounds, or scaled.</param>
    extension(double input)
    {
        /// <summary>
        /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the maximum bound can also result in a true result.
        /// </summary>
        ///  <param name="min">The minimum value of the number.</param>
        ///  <param name="max">The maximum value of the number (inclusive or exclusive, depending on <paramref name="maxIsInclusive"/>).</param>
        ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
        public bool IsInRange(double min, double max, bool maxIsInclusive = true)
            => maxIsInclusive ? input <= max && input >= min : input < max && input >= min;

        /// <summary>
        /// Keeps a double within the given bounds, clipping it if it falls outside.
        /// </summary>
        /// <remarks>
        /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
        /// </remarks>
        ///  <param name="floor">The lowest acceptable value.</param>
        ///  <param name="ceiling">The highest acceptable value.</param>
        public double KeepInRange(double floor = double.MinValue, double ceiling = double.MaxValue)
        {
            if (input > ceiling)
            {
                return ceiling;
            }

            return input < floor || double.IsNaN(input) ? floor : input;
        }

        //Scale in a similar way to Ventuz
        public double Scale(double minIn, double maxIn, double minOut, double maxOut)
        {
            double scaled = minOut + ((input - minIn) / (maxIn - minIn) * (maxOut - minOut));
            return scaled.KeepInRange(Math.Min(minOut, maxOut), Math.Max(minOut, maxOut));
        }
    }

    /// <param name="input">The number to be compared.</param>
    extension(double? input)
    {
        public bool IsInRange(double min, double max, bool maxIsInclusive = true)
            => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);
    }

    /// <param name="number">The number to be described.</param>
    extension(double number)
    {
        public string PluralIfRequired(string singularNoun, string pluralSuffix = "s")
            => $"{number} {singularNoun}{(number == 1d ? "" : pluralSuffix)}";
    }

    // This remains a classic extension method: an extension block does not emit DecimalConstantAttribute for the
    // decimal default values, which would change the defaults seen by callers in other assemblies.
    /// <summary>
    /// Keeps a decimal within the given bounds, clipping it if it falls outside.
    /// </summary>
    /// <param name="input">The number to be kept in bounds.</param>
    ///  <param name="floor">The lowest acceptable value.</param>
    ///  <param name="ceiling">The highest acceptable value.</param>
    public static decimal KeepInRange(this decimal input, decimal floor = decimal.MinValue, decimal ceiling = decimal.MaxValue)
    {
        if (input > ceiling)
        {
            return ceiling;
        }

        return input < floor ? floor : input;
    }

    /// <param name="input">The timespan to be compared or kept in bounds.</param>
    extension(TimeSpan input)
    {
        /// <summary>
        /// Returns a Boolean indicating whether the given timespan falls between the other two.  Optionally, timespans equal to the maximum bound can also result in a true result.
        /// </summary>
        ///  <param name="min">The minimum value of the timespan.</param>
        ///  <param name="max">The maximum value of the timespan (inclusive or exclusive, depending on <paramref name="maxIsInclusive"/>).</param>
        ///  <param name="maxIsInclusive">Specifies whether timespans equal to the maximum should give a true result.</param>
        public bool IsInRange(TimeSpan min, TimeSpan max, bool maxIsInclusive = true)
            => maxIsInclusive ? input <= max && input >= min : input < max && input >= min;

        /// <summary>
        /// Keeps a timespan within the given bounds, clipping it if it falls outside.
        /// </summary>
        ///  <param name="floor">The lowest acceptable value.</param>
        ///  <param name="ceiling">The highest acceptable value.</param>
        public TimeSpan KeepInRange(TimeSpan? floor = null, TimeSpan? ceiling = null)
        {
            if (ceiling.HasValue && input > ceiling.Value)
            {
                return ceiling.Value;
            }

            return floor.HasValue && input < floor.Value ? floor.Value : input;
        }
    }

    /// <param name="input">The timespan to be compared.</param>
    extension(TimeSpan? input)
    {
        /// <summary>
        /// Returns a Boolean indicating whether the given timespan is non-null and falls between the other two.  Optionally, timespans equal to the maximum bound can also result in a true result.
        /// </summary>
        ///  <param name="min">The minimum value of the timespan.</param>
        ///  <param name="max">The maximum value of the timespan (inclusive or exclusive, depending on <paramref name="maxIsInclusive"/>).</param>
        ///  <param name="maxIsInclusive">Specifies whether timespans equal to the maximum should give a true result.</param>
        public bool IsInRange(TimeSpan min, TimeSpan max, bool maxIsInclusive = true)
            => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);
    }

    /// <param name="timespan">The timespan to be described.</param>
    extension(TimeSpan timespan)
    {
        public string ToSentence()
            => ToSentence((TimeSpan?)timespan);
    }

    /// <param name="timespan">The timespan to be described.</param>
    extension(TimeSpan? timespan)
    {
        public string ToSentence()
        {
            if (!timespan.HasValue)
            {
                return "never";
            }

            TimeSpan timeVal = timespan.Value;
            string?[] candidateParts = [GetPart(timeVal.Days, "day"), GetPart(timeVal.Hours, "hour"), GetPart(timeVal.Minutes, "minute")];
            string[] dayParts = [.. candidateParts.OfType<string>()];

            return dayParts switch
            {
                [] => "a few seconds",
                [_] => dayParts[0],
                _ => string.Join(", ", dayParts[..^1]) + " and " + dayParts[^1],
            };

            static string? GetPart(double n, string s)
                => n > 0 ? PluralIfRequired(n, s) : null;
        }
    }

    /// <param name="timeSpan">The timespan to be serialized.</param>
    extension(TimeSpan timeSpan)
    {
        /// <summary>
        /// Convert a TimeSpan to a string with decimal places using intuitive rounding rules, unlike string.Format.
        /// Only useful when you need at least minutes, seconds and decimals.
        /// </summary>
        /// <param name="includeHour">If false, the hour data will be rolled into the minutes.</param>
        /// <param name="minuteMinimumDigits">The minimum number of digits to include in the minutes portion.</param>
        /// <param name="decimalPlaces">The number of decimal places for the seconds portion.</param>
        public string ToString(bool includeHour, int minuteMinimumDigits, int decimalPlaces)
        {
            StringBuilder stringBuilder = new(32);
            int minutes;
            if (includeHour)
            {
                int hours = (timeSpan.Days * 24) + timeSpan.Hours;
                _ = stringBuilder.AppendFormat("{0:D1}", hours);
                _ = stringBuilder.Append(':');
                minutes = timeSpan.Minutes;
            }
            else
            {
                minutes = (timeSpan.Days * 1440) + (timeSpan.Hours * 60) + timeSpan.Minutes;
            }
            double seconds = Math.Round(timeSpan.Seconds + ((double)timeSpan.Milliseconds / 1000), decimalPlaces);
            _ = stringBuilder.AppendFormat($"{{0:D{minuteMinimumDigits}}}:{(seconds < 10 ? "0" : "")}{{1:F{decimalPlaces}}}", minutes, seconds);
            return stringBuilder.ToString();
        }
    }

    /// <param name="input">The string to process.</param>
    extension(string input)
    {
        public string ReplaceSmartChars()
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (s_replacements.TryGetValue(chars[i], out char value))
                {
                    chars[i] = value;
                }
            }
            return new string(chars);
        }

        public string MakeValidFilename()
        {
            char[] chars = input.ToCharArray();
            bool changed = false;
            for (int i = 0; i < chars.Length; i++)
            {
                if (s_invalidFileNameChars.Contains(chars[i]))
                {
                    chars[i] = '#';
                    changed = true;
                }
            }
            return changed ? new string(chars) : input;
        }

        /// <summary>
        /// If the supplied string is mostly upper case, returns true. If not, returns false. May return null if there is not enough data to determine case.
        /// </summary>
        /// <param name="allowedLowerPercent">The largest fraction (0 to 1) of the string's letters that may be lower case for it to still count as upper case.</param>
        /// <returns>True if mostly upper case, false if not (including when there are no upper-case letters at all), or null if the counts are too close to call.</returns>
        public bool? IsUpperCase(double allowedLowerPercent = .50)
        {
            if (input is null)
            {
                return null;
            }
            int upperCount = 0;
            int lowerCount = 0;
            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    upperCount++;
                }
                else if (char.IsLower(c))
                {
                    lowerCount++;
                }
            }
            if (upperCount == 0)
            {
                return false;
            }
            else if (Math.Abs(lowerCount - upperCount) < 3)
            {
                return null;
            }
            int upperLowerCount = upperCount + lowerCount;
            return (((float)lowerCount) / upperLowerCount) <= allowedLowerPercent;
        }
    }

    /// <param name="text">The camel-case text to convert.</param>
    extension(string text)
    {
        public string CamelCaseToSentence(bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            StringBuilder newText = new(text.Length * 2);
            _ = newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    {
                        _ = newText.Append(' ');
                    }
                }

                _ = newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }

    /// <param name="relativePath">A path relative to the base path, or null to use the base path itself.</param>
    extension(string? relativePath)
    {
        public string MakePathAbsolute(Uri basePath)
            => (relativePath is null ? basePath : new Uri(basePath, relativePath)).LocalPath;
    }

    /// <param name="value">The character to check.</param>
    extension(char value)
    {
        public bool IsSpaceOrTab() => s_spacesAndTabs.Contains(value);
    }

    /// <param name="value">The Enum value.</param>
    extension(Enum? value)
    {
        /// <summary>
        /// Extension method which returns the string specified in the Description attribute of an Enum, if any.  Otherwise, name is returned.
        /// </summary>
        /// <returns></returns>
        public string Description()
        {
            if (value is not null)
            {
                object[]? attrs = value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs?.Length > 0 && attrs[0] is DescriptionAttribute attr)
                {
                    return attr.Description;
                }
            }
            //Fallback
            return value?.ToString()?.Replace("_", " ") ?? "null";
        }
    }

    /// <param name="value">The Enum value.</param>
    extension(Enum value)
    {
        public object UnderlyingValue()
            => Convert.ChangeType(value, value.GetTypeCode());
    }

    /// <param name="value">The value to check.</param>
    /// <typeparam name="T">The type of the enum.</typeparam>
    extension<T>(T? value) where T : struct, Enum
    {
        /// <summary>
        /// Return the supplied enum member if it is non-null and defined, otherwise return a defined default.
        /// </summary>
        /// <param name="defaultValue">The default value to be returned if the value is not defined.</param>
        /// <returns></returns>
        public T EnumFallbackToDefault(T defaultValue)
            => !value.HasValue || !Enum.IsDefined(value.Value) ? defaultValue : value.Value;
    }

    /// <summary>
    /// Return true and output safe reference to the object if it's not null.
    /// </summary>
    /// <typeparam name="T">The type of object to check.</typeparam>
    /// <param name="toCheck">The object to check.</param>
    /// <param name="valueNotNull">The null-checked version of the object</param>
    public static bool SafeNullCheck<T>(this T? toCheck, [NotNullWhen(true)] out T? valueNotNull) where T : class
    {
        if (toCheck is null)
        {
            valueNotNull = null;
            return false;
        }
        valueNotNull = toCheck;
        return true;
    }

    /// <param name="toCheck">The object to check.</param>
    /// <typeparam name="T">The type of object to check.</typeparam>
    extension<T>(T? toCheck) where T : struct
    {
        /// <summary>
        /// Return true and output safe reference to the object if it's not null.
        /// </summary>
        /// <param name="valueNotNull">The null-checked version of the object</param>
        public bool SafeNullCheck([NotNullWhen(true)] out T? valueNotNull)
        {
            if (toCheck is null)
            {
                valueNotNull = null;
                return false;
            }
            valueNotNull = toCheck;
            return true;
        }
    }

    /// <param name="source">The <see cref="StringBuilder"/> to containing the character.</param>
    extension(StringBuilder source)
    {
        /// <summary>
        /// Return the element at the given index. If it is out of range, return null.
        /// </summary>
        /// <param name="index">The index of the character within the <see cref="StringBuilder"/>.</param>
        public char? ElementAtOrNull(int index)
            => index.IsInRange(0, source.Length, false) ? source[index] : null;
    }

    /// <param name="task">The task to be executed.</param>
    extension(Task task)
    {
        /// <summary>
        /// A simple method to allow a task to be fired off from synchronous code which will never be seen again unless an exception needs to be handled.
        /// </summary>
        /// <param name="exceptionHandler">A delegate which will handle and exception object in case an exception is raised.</param>
        public async void FireAndForgetSafeAsync(Action<Exception>? exceptionHandler = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                exceptionHandler?.Invoke(ex);
            }
        }
    }

    /// <summary>
    /// Run a task synchronously, returning the result.
    /// </summary>
    public static T SyncResult<T>(this Task<T> task)
    {
        task.RunSynchronously();
        return task.Result;
    }

    /// <param name="action">The task, which may be null.</param>
    extension(Task? action)
    {
        /// <summary>
        /// Invoke the given action if it is not null. Useful when raising events that return a Task.
        /// </summary>
        public Task InvokeUnlessNull()
            => action ?? Task.CompletedTask;
    }

    /// <param name="assembly">The assembly to inspect.</param>
    extension(Assembly assembly)
    {
        /// <summary>
        /// Get the build data of an assembly which has been built with a SourceRevisionID formatted in a standardized way (see remarks).
        /// If you also need to get the version number, consider using <seealso cref="GetVersionAndBuildDate"></seealso> instead.
        /// </summary>
        /// <remarks>Required in project file: <![CDATA[<SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("O"))</SourceRevisionId>]]>
        /// </remarks>
        public DateTime GetBuildDate()
            => GetVersionAndBuildDate(assembly).Item2 ?? default;

        /// <summary>
        /// Get the product version and build date of the assembly containing the given type. The build date is only available if the assembly was built with a SourceRevisionID formatted in a standardized way (see remarks).
        /// </summary>
        /// <remarks>Required in project file: <![CDATA[<SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("O"))</SourceRevisionId>]]>
        /// </remarks>
        public (string, DateTime?) GetVersionAndBuildDate()
        {
            const string BuildVersionMetadataPrefix = "+build";

            AssemblyInformationalVersionAttribute? attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion is not null)
            {
                string value = attribute.InformationalVersion;
                int index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    string versionPart = value[..index];
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    if (DateTime.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    {
                        return (versionPart, result);
                    }
                    return (versionPart, null);
                }
                return (value, null);
            }
            return (assembly.GetName().Version?.ToString() ?? "unknown", null);
        }

        /// <summary>
        /// Get the product version of the given assembly. This is a string which may contain non-numeric characters. Any build date information is stripped out.
        /// </summary>
        public string GetProductVersion()
            => GetVersionAndBuildDate(assembly).Item1;
    }

    /// <param name="address">The address to check.</param>
    extension(IPAddress address)
    {
        public bool IsMulticast()
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return address.IsIPv6Multicast;
            }
            byte[] bytes = address.GetAddressBytes();
            return bytes[0] is > 223 and < 240;
        }
    }

    /// <param name="type">The type to check.</param>
    extension(Type type)
    {
        /// <summary>
        /// Returns true if supplied type is either Type or the runtime Type implementation.
        /// </summary>
        public bool IsTypeOrRuntimeType()
            => type == typeof(Type) || type == s_runtimeType;
    }

    /// <param name="type">The type to check, which may be null.</param>
    extension(Type? type)
    {
        /// <summary>
        /// Use reflection to determine whether the type is a record type. Results are cached in memory to improve future performance.
        /// </summary>
        public bool IsRecordType()
        {
            if (type is null)
            {
                return false;
            }
            return s_isRecordTypeCache.GetOrAdd(type, static t => t.GetMethods().Any(static m => m.Name == "<Clone>$"));
        }
    }

    // Rooted here so that the timer is neither disposed nor collected before it fires.
    private static Timer? s_processTerminateWatchdog;

    /// <summary>
    /// Kill the current process if it is still running after the given time. Call this at the end of an orderly shutdown to
    /// guarantee that the process exits even if something is keeping it alive. Only the first call starts the countdown.
    /// </summary>
    /// <param name="logger">If supplied, used to log the start of the wait and the kill.</param>
    /// <param name="timeout">How long to allow the process to exit by itself. Defaults to five seconds.</param>
    public static void StartProcessTerminateWatchdog(ILogger? logger = null, TimeSpan? timeout = null)
    {
        TimeSpan defaultedTimeout = timeout ?? TimeSpan.FromSeconds(5);
        Timer nuker = new(OnTimeout, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        if (Interlocked.CompareExchange(ref s_processTerminateWatchdog, nuker, null) is not null)
        {
            // A countdown is already running; the earlier deadline stands.
            nuker.Dispose();
            return;
        }
        _ = nuker.Change(defaultedTimeout, Timeout.InfiniteTimeSpan);
        logger?.LogWatchdogWaiting(defaultedTimeout);

        void OnTimeout(object? state)
        {
            logger?.LogProcessTimedOut();
            Process.GetCurrentProcess().Kill();
        }
    }

    private static readonly FrozenDictionary<char, char> s_replacements = new Dictionary<char, char>
    {
        { '–', '-' }, //Em-dash
        { '—', '-' }, //two em-dash
        { '―', '-' }, //Horizontal bar
        { '‗', '_' }, //Double low line
        { '‘', '\'' }, //Left single quotation mark
        { '’', '\'' }, //Right single quotation mark
        { '‚', ',' }, //Single low quotation mark
        { '‛', '\'' }, //Single high reversed quotation mark
        { '“', '\"' }, //Left double quotation mark
        { '”', '\"' }, //Right double quotation mark
        { '„', '\"' }, //Double low quotation mark
        { '‟', '\"' }, //Double high reversed quotation mark
        { '′', '\'' }, //Prime
        { '″', '\"' }, //Double prime
    }.ToFrozenDictionary();

    private static readonly FrozenSet<char> s_invalidFileNameChars = [.. Path.GetInvalidFileNameChars()];

    private static readonly FrozenSet<char> s_spacesAndTabs = [' ', ' ', '\t'];

    public static string? UpperCaseIfRequired(string? input, double allowedLowerPercent = .50, bool upperIfUnknown = true)
    {
        if (input is null)
        {
            return null;
        }

        // Mostly upper-case text is left alone so that deliberate lower-case letters survive; mostly lower-case text is
        // upper-cased. Text with no upper-case letters at all is also left alone, as it always has been.
        bool makeUpper = IsUpperCase(input, allowedLowerPercent) switch
        {
            null => upperIfUnknown,
            true => false,
            false => input.Any(char.IsUpper),
        };
        return makeUpper ? input.ToUpper() : input;
    }

    /// <summary>
    /// Ensure the given generic list is sized within the specified range, creating it if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureAndCreateListCount<T>(ref IList<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        list ??= [];
        EnsureListCount(list, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureOrCreateListCount<T>(ref List<T> list, int minCount, Func<int, T> factory)
    {
        list ??= [];
        EnsureListCount(list, minCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list contains at least <paramref name="minCount"/> and at most <paramref name="maxCount"/> entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureOrCreateListCount<T>(ref ObservableCollection<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        list ??= [];
        EnsureListCount(list, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list contains at least <paramref name="minCount"/> and at most <paramref name="maxCount"/> entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureOrCreateListCount<T>(ref ObservableCollectionEx<T> list, int minCount, int maxCount, Func<int, T> factory) where T : INotifyPropertyChanged
    {
        list ??= [];
        EnsureListCount(list, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list is sized within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureOrCreateListCount<T>(ref List<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        list ??= [];
        EnsureListCount(list, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given binding list is sized within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="maxCount">The maximum number of entries allowed.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureOrCreateListCount<T>(ref BindingList<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        list ??= [];
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Return a copy of the given string with characters added to or removed from the end until it matches the given target length.
    /// </summary>
    /// <param name="original">The input string of uncertain length.</param>
    /// <param name="targetLength">The required length of the returned string.</param>
    /// <param name="padding">The character to add, if required.</param>
    /// <returns></returns>
    public static string EnsureStringLength(string original, int targetLength, char padding)
    {
        int diff = targetLength - original.Length;
        if (diff == 0)
        {
            return original;
        }
        else if (diff < 0)
        {
            return original[..targetLength];
        }
        else
        {
            StringBuilder sb = new(original);
            _ = sb.Append(padding, diff);
            return sb.ToString();
        }
    }

    public static bool RelaunchAsAdministrator()
    {
        ProcessStartInfo info = new()
        {
            FileName = Assembly.GetCallingAssembly().Location,
            UseShellExecute = true,
            Verb = "runas"
        };
        try
        {
            return Process.Start(info) is not null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Add ".broken" to the end of a file name, incrementing the number if there is already a file with that name.
    /// </summary>
    public static void RenameBrokenFile(string path, ILogger? logger = null)
    {
        string? dir = Path.GetDirectoryName(path);
        string pathNoExt = Path.GetFileNameWithoutExtension(path);
        string ext = Path.GetExtension(path);
        for (int i = 1; i < 100; i++)
        {
            string newPath = Path.Combine(dir ?? "", $"{pathNoExt}.broken.{i}{ext}");
            if (!File.Exists(newPath))
            {
                try
                {
                    File.Copy(path, newPath);
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    logger?.LogRenameBrokenFileFailed(ex, path);
                    return;
                }
                logger?.LogRenamedBrokenFile(newPath);
                return;
            }
        }
    }

    /// <summary>
    /// Create a directory for a given path if it does not exist. Path may be to a file or directory.
    /// </summary>
    /// <param name="path">A path to a directory, or a file in a directory.</param>
    /// <returns>True if directory existed or was created, otherwise false.</returns>
    public static bool EnsureDirectory([NotNullWhen(true)] string? path)
    {
        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            try
            {
                _ = Directory.CreateDirectory(dir);
                return true;
            }
            catch
            {
            }
        }
        return false;
    }

    //Linear interpolation
    public static double Lerp(double start, double end, double progress)
    {
        progress = progress.KeepInRange(0, 1);
        return (float)((start * (1 - progress)) + (end * progress));
    }

    private static readonly ImmutableArray<string> s_unitsMap = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"];
    private static readonly ImmutableArray<string> s_tensMap = ["zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"];

    public static string? EnumerableToDelimitedString(object? value, string delimiter, int offset = 0, bool includeEmptyItems = false, string nullNumberPlaceholder = "", string nullStringPlaceholder = "NullPlaceholder")
    {
        if (value is null)
        {
            return null;
        }
        StringBuilder sb = new();
        if (value is IEnumerable<bool> bools)
        {
            foreach (bool b in bools)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(b ? "1" : "0");
            }
        }
        else if (value is IEnumerable<bool?> nbools)
        {
            foreach (bool? b in nbools)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(b.HasValue ? (b.Value ? "1" : "0") : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<string> strings)
        {
            foreach (string s in strings)
            {
                if (includeEmptyItems || !string.IsNullOrWhiteSpace(s))
                {
                    _ = sb.Append(delimiter);
                    _ = sb.Append(s);
                }
            }
        }
        else if (value is IEnumerable<string?> nstrings)
        {
            foreach (string? s in nstrings)
            {
                if (includeEmptyItems || !string.IsNullOrWhiteSpace(s))
                {
                    _ = sb.Append(delimiter);
                    _ = sb.Append(s ?? nullStringPlaceholder);
                }
            }
        }
        else if (value is IEnumerable<int> ints)
        {
            foreach (int i in ints)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i + offset);
            }
        }
        else if (value is IEnumerable<int?> nints)
        {
            foreach (int? i in nints)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i.HasValue ? i.Value + offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<long> longs)
        {
            foreach (long i in longs)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i + offset);
            }
        }
        else if (value is IEnumerable<long?> nlongs)
        {
            foreach (long? i in nlongs)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i.HasValue ? i.Value + offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<uint> uints)
        {
            foreach (uint i in uints)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i + (uint)offset);
            }
        }
        else if (value is IEnumerable<uint?> nuints)
        {
            foreach (uint? i in nuints)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i.HasValue ? i.Value + offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<ulong> ulongs)
        {
            foreach (ulong i in ulongs)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i + (ulong)offset);
            }
        }
        else if (value is IEnumerable<ulong?> nulongs)
        {
            foreach (ulong? i in nulongs)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i.HasValue ? i.Value + (ulong)offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<IEnumerable<int>> intList)
        {
            foreach (IEnumerable<int> intItems in intList)
            {
                bool first = true;
                _ = sb.Append(delimiter);
                foreach (int i in intItems)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        _ = sb.Append(',');
                    }
                    _ = sb.Append(i + offset);
                }
            }
        }
        else if (value is IEnumerable<float?> nfloats)
        {
            foreach (float? i in nfloats)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i.HasValue ? (i.Value + offset).ToString("n3") : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<float> floats)
        {
            foreach (float i in floats)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append((i + offset).ToString("n3"));
            }
        }
        else if (value is IEnumerable<double?> ndoubles)
        {
            foreach (double? i in ndoubles)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append(i.HasValue ? (i.Value + offset).ToString("n3") : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<double> doubles)
        {
            foreach (double i in doubles)
            {
                _ = sb.Append(delimiter);
                _ = sb.Append((i + offset).ToString("n3"));
            }
        }
        else
        {
            return null;
        }
        return sb.Length <= delimiter.Length ? string.Empty : sb.ToString(delimiter.Length, sb.Length - delimiter.Length);
    }

    public static IEnumerable<string>? DelimitedStringToNonNullableString(string delimited, string delimiter, string nullStringPlaceholder = "?", bool exceptionOnFailure = false)
    {
        IEnumerable<string?>? result = DelimitedStringToNullableString(delimited, delimiter, nullStringPlaceholder);
        return exceptionOnFailure && result?.Any(static t => t is null) == true
            ? throw new Exception("String parsing failed")
            : (result?.Select(t => t ?? nullStringPlaceholder));
    }

    public static IEnumerable<string?>? DelimitedStringToNullableString(string delimited, string delimiter, string? nullStringPlaceholder = "?")
    {
        if (string.IsNullOrWhiteSpace(delimited))
        {
            return null;
        }
        string[] parts = delimited.Split([delimiter], StringSplitOptions.None);
        return parts
            .Select(static p => p.Length > 1 && p[0] == ' ' ? p.TrimStart() : p)
            .Select(p => string.IsNullOrEmpty(p) || p == nullStringPlaceholder ? null : p);
    }

    public static IEnumerable<T>? DelimitedStringToNonNullableType<T>(string delimited, string delimiter, int offset = 0, bool exceptionOnFailure = false)
        where T : struct
    {
        IEnumerable<T?>? result = DelimitedStringToNullableType<T>(delimited, delimiter, offset);
        return exceptionOnFailure && result?.Any(static t => t is null) == true
            ? throw new Exception("String parsing failed")
            : (result?.WhereNotNull().Select(static t => t));
    }

    public static IEnumerable<T?>? DelimitedStringToNullableType<T>(string delimited, string delimiter, int offset = 0)
        where T : struct
    {
        if (string.IsNullOrWhiteSpace(delimited))
        {
            return null;
        }
        string[] parts = delimited.Split([delimiter.Trim()], StringSplitOptions.None);
        Type targetType = typeof(T);
        //Todo: special handling for strings, because they won't main it through constraint
        if (targetType.IsAssignableFrom(typeof(int)))
        {
            IEnumerable<int?> result = parts.Select<string, int?>(s => int.TryParse(s.Trim(), out int i) ? i - offset : null);
            return (IEnumerable<T?>)result;
        }
        else if (targetType.IsAssignableFrom(typeof(uint)))
        {
            IEnumerable<uint?> result = parts.Select<string, uint?>(s => uint.TryParse(s.Trim(), out uint i) ? i - (uint)offset : null);
            return (IEnumerable<T?>)result;
        }
        else if (targetType.IsAssignableFrom(typeof(long)))
        {
            IEnumerable<long?> result = parts.Select<string, long?>(s => long.TryParse(s.Trim(), out long i) ? i - offset : null);
            return (IEnumerable<T?>)result;
        }
        else if (targetType.IsAssignableFrom(typeof(ulong)))
        {
            IEnumerable<ulong?> result = [.. parts.Select<string, ulong?>(s => ulong.TryParse(s.Trim(), out ulong i) ? i - (ulong)offset : null)];
            return (IEnumerable<T?>)result;
        }
        else if (targetType.IsAssignableFrom(typeof(bool)))
        {
            IEnumerable<bool?> result = parts.Select<string, bool?>(static s => bool.TryParse(s.Trim(), out bool i) ? i : null);
            return (IEnumerable<T?>)result;
        }
        return null;
    }

    /// <summary>
    /// Get the product version of the assembly containing the given type. This is a string which may contain non-numeric characters. Any build date information is stripped out.
    /// </summary>
    public static string GetProductVersion<T>()
        => GetProductVersion(typeof(T).Assembly);

    /// <summary>
    /// Return a shuffled sequence of numbers, ensuring the first in the sequence does not equal the given number.
    /// </summary>
    /// <param name="sequenceLength">The length of the sequence.</param>
    /// <param name="rnd">The random number generator to use.  If null, use default instance.</param>
    /// <param name="disallowedStart">A number which will not be allowed in the first position.</param>
    public static int[] ShuffledNumbers(int sequenceLength, Random? rnd, int disallowedStart)
    {
        Random random = rnd ?? s_rnd;
        int[] shuffle = ShuffledNumbers(sequenceLength, random);
        if (shuffle[0] == disallowedStart)
        {
            int swap = random.Next(1, sequenceLength);
            shuffle[0] = shuffle[swap];
            shuffle[swap] = disallowedStart;
        }
        return shuffle;
    }

    /// <summary>
    /// Return a shuffled sequence of numbers in the range [0, <paramref name="count"/>).
    /// </summary>
    /// <param name="count">The length of the sequence.</param>
    /// <param name="rnd">The random number generator to use. If null, use default instance.</param>
    public static int[] ShuffledNumbers(int count, Random? rnd)
    {
        int[] indices = [.. Enumerable.Range(0, count)];
        Random random = rnd ?? s_rnd;
        for (int remaining = count; remaining > 0; --remaining)
        {
            int k = random.Next(remaining);
            (indices[k], indices[remaining - 1]) = (indices[remaining - 1], indices[k]);
        }
        return indices;
    }

    /// <summary>
    /// Return a shuffled sequence of numbers, preventing the sequence for matching any of these supplied.
    /// </summary>
    /// <param name="sequenceLength">The length of the sequence.</param>
    /// <param name="rnd">The random number generator to use. If null, use default instance.</param>
    /// <param name="preventedResults">A set of result arrays that should be disallowed.</param>
    public static int[] ShuffledNumbers(int sequenceLength, Random? rnd, HashSet<int[]> preventedResults)
    {
        //There are more efficient ways to do this, but none simpler.
        //Could get stuck if set of prevented results is >= sequenceLength factorial
        while (true)
        {
            int[] result = ShuffledNumbers(sequenceLength, rnd);
            if (!preventedResults.Contains(result))
            {
                return result;
            }
        }
    }

    internal class HashSetWithIndex(IEnumerable<int> source)
    {
        private readonly HashSet<int> _hashset = [.. source];
        private readonly List<int> _list = [.. source];
        internal void Add(int item)
        {
            _ = _hashset.Add(item);
            _list.Add(item);
        }

        internal int GetAndRemove(int index)
        {
            int item = _list[index];
            _ = _hashset.Remove(item);
            _list.RemoveAt(index);
            return item;
        }

        internal void RemoveKey(int key)
        {
            if (_hashset.Remove(key))
            {
                //maybe a refactor could remove this slow operation
                _ = _list.Remove(key);
            }
        }

        internal int Count => _list.Count;
    }

    /// <summary>
    /// Return a list of items of a given length, each taken from a source pool. A minimum repeat distance is observed to ensure matching results are not returned too close to each other.
    /// </summary>
    /// <param name="sourceLength">The number of values to choose from, including zero.</param>
    /// <param name="destinationLength">The length of the output array.</param>
    /// <param name="minimumRepeatDistance">The minimum distance enforced between matching items.</param>
    /// <param name="loopback">If true, items at the end out the sequence are also checked against items at the start.</param>
    /// <param name="rnd">The random number generator to use. If null, use default instance.</param>
    /// <returns></returns>
    public static ImmutableArray<int> RandomSequenceWithMinimumDistance(int sourceLength, int destinationLength, int minimumRepeatDistance, bool loopback, Random? rnd)
    {
        if (minimumRepeatDistance < 0)
        {
            throw new ArgumentException("Minimum repeat distance must be a non-negative value.");
        }
        if (minimumRepeatDistance > sourceLength)
        {
            throw new ArgumentException("Minimum repeat distance cannot be longer than the sourcePool.");
        }
        int[] sourceValues = [.. Enumerable.Range(0, sourceLength)];
        Random random = rnd ?? s_rnd;
        ImmutableArray<int>.Builder output = ImmutableArray.CreateBuilder<int>(destinationLength);
        HashSet<int> backWindow = [];
        HashSet<int>? forwardWindow = loopback ? [] : null;
        int randomIndex;
        while (output.Count < destinationLength)
        {
            int overshoot = output.Count + minimumRepeatDistance - destinationLength;
            if (overshoot > 0)
            {
                _ = forwardWindow?.Add(output[overshoot - 1]);
            }
            if (backWindow.Count > minimumRepeatDistance || (overshoot > 0 && (output.Count - minimumRepeatDistance) >= 1))
            {
                _ = backWindow.Remove(output[output.Count - minimumRepeatDistance - 1]);
            }
            //Testing shows this is faster than ensuring that the random index is within only valid items
            if (backWindow.Count + (forwardWindow?.Count ?? 0) >= sourceLength)
            {
                //It's possible that finding a strictly valid item is impossible, causing an infinite loop, so use alternative method
                int[] remaining = [.. sourceValues.Where(i => !backWindow.Contains(i) && forwardWindow?.Contains(i) != true)];
                if (remaining.Length == 0)
                {
                    //no other options. We're going to have to break the rule
                    randomIndex = output[output.Count / 2];
                }
                else
                {
                    randomIndex = remaining.Length == 1 ? remaining[0] : remaining[random.Next(remaining.Length)];
                }
            }
            else
            {
                do
                {
                    randomIndex = random.Next(sourceLength);
                } while (backWindow.Contains(randomIndex) || forwardWindow?.Contains(randomIndex) == true);
            }
            output.Add(randomIndex);
            _ = backWindow.Add(randomIndex);
        }
        return output.ToImmutable();
    }

    private sealed class IntArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[]? x, int[]? y)
            => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

        public int GetHashCode([DisallowNull] int[] obj)
            => StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
    }

    /// <summary>
    /// Return multiple unique variations of a shuffled sequence of numbers.
    /// </summary>
    /// <param name="sequenceLength">The length of each sequence.</param>
    /// <param name="maxCount">The maximum number of variations.</param>
    /// <param name="rnd">The random number generator to use. If null, use default instance.</param>
    public static ImmutableArray<int[]> ShuffledNumbersPermutations(int sequenceLength, int maxCount, Random? rnd)
    {
        int maxPossible = Factorial((ulong)sequenceLength).NarrowToIntClamped();
        if (maxCount > maxPossible)
        {
            //special case where we're going to want every possible combination
            return AllShuffledNumbersPermutations(sequenceLength, maxPossible);
        }
        else
        {
            ImmutableArray<int[]>.Builder builder = ImmutableArray.CreateBuilder<int[]>(maxCount);
            HashSet<int[]> uniqueVariations = [with(maxCount, new IntArrayComparer())];
            for (int i = 0; i < maxCount; i++)
            {
                while (true)
                {
                    int[] result = ShuffledNumbers(sequenceLength, rnd, uniqueVariations);
                    if (!uniqueVariations.Contains(result))
                    {
                        builder.Add(result);
                        _ = uniqueVariations.Add(result);
                        break;
                    }
                }
            }
            return builder.ToImmutable();
        }
    }

    /// <summary>
    /// Return every unique variation of a shuffled sequence of numbers.
    /// </summary>
    /// <remarks>Based on Heap's Algorithm.</remarks>
    /// <param name="sequenceLength">The length of the sequence to be shuffled. Using this for numbers greater than 8 is not recommended.</param>
    public static ImmutableArray<int[]> ShuffledNumbersPermutations(int sequenceLength)
    {
        int maxPossible = Factorial((ulong)sequenceLength).NarrowToIntClamped();
        return AllShuffledNumbersPermutations(sequenceLength, maxPossible);
    }

    private static ImmutableArray<int[]> AllShuffledNumbersPermutations(int sequenceLength, int permutationCount)
    {
        if (sequenceLength <= 0)
        {
            return [];
        }
        ImmutableArray<int[]>.Builder permutations = ImmutableArray.CreateBuilder<int[]>(permutationCount);
        int[] indexes = new int[sequenceLength];
        int[] items = [.. Enumerable.Range(0, sequenceLength)];
        int[] identity = [.. items];
        // Heap's algorithm yields every arrangement except the starting one, one swap at a time.
        for (int i = 1; i < sequenceLength && permutations.Count < permutationCount - 1;)
        {
            if (indexes[i] < i)
            {
                if ((i & 1) == 1)
                {
                    Swap(ref items[i], ref items[indexes[i]]);
                }
                else
                {
                    Swap(ref items[i], ref items[0]);
                }

                permutations.Add([.. items]);

                indexes[i]++;
                i = 1;
            }
            else
            {
                indexes[i++] = 0;
            }
        }
        // The in-order sequence comes last.
        permutations.Add(identity);
        return permutations.ToImmutable();

        static void Swap(ref int a, ref int b)
            => (b, a) = (a, b);
    }

    /// <summary>
    /// Return a random number excluding one value. Execution time should be consistent.
    /// </summary>
    /// <param name="minimum">Inclusive lower bound.</param>
    /// <param name="maximumExclusive">The exclusive upper bound.</param>
    /// <param name="excluding">The integer to exclude.</param>
    /// <param name="rnd">The random number generator to use.  If null, use default instance.</param>
    public static int RandomExcluding(int minimum, int maximumExclusive, int excluding, Random? rnd)
    {
        Random random = rnd ?? s_rnd;
        int r = random.Next(minimum, maximumExclusive - 1);
        return r >= excluding ? r + 1 : r;
    }

    private static readonly Random s_rnd = new();

    /// <summary>
    /// Generate a random string of the requested length consisting only of upper-case alphabetic characters.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string RandomAlphabeticString(int length)
    {
        StringBuilder sb = new(length);
        for (int i = 0; i < length; i++)
        {
            _ = sb.Append((char)s_rnd.Next(65, 91));
        }
        return sb.ToString();
    }

    private static readonly Type s_runtimeType = typeof(Type).GetType(); //Trick to get an instance of an internal object.

    private static readonly ConcurrentDictionary<Type, bool> s_isRecordTypeCache = [];

    public static T[] ArrayRepeat<T>(T value, int count)
    {
        T[] array = new T[count];
        Array.Fill(array, value);
        return array;
    }

    public static void EnforceMinimumUptime(TimeSpan minimum, ILogger logger, CancellationToken cancellationToken)
    {
        TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        TimeSpan waitTime = minimum - uptime;
        if (waitTime > TimeSpan.Zero)
        {
            logger.LogDelayingForMinimumUptime(waitTime.TotalSeconds);
            Task.Delay(waitTime, cancellationToken).Wait(cancellationToken);
            logger.LogMinimumUptimeDelayComplete();
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogMinimumUptimeAlreadyMet(uptime.ToSentence());
        }
    }

    [GeneratedRegex(@",\s*(Version|Culture|PublicKeyToken)=[^\],]*")]
    private static partial Regex StripDownTypeName();

    /// <summary>
    /// Strip down an assembly qualified type name to leave only the assembly and type names, not the version, culture, or PublicKeyToken.
    /// </summary>
    [return: NotNullIfNotNull(nameof(assemblyQualifiedName))]
    public static string? IsolateAssemblyAndTypeName(string? assemblyQualifiedName)
        => assemblyQualifiedName is null ? null : StripDownTypeName().Replace(assemblyQualifiedName, "");

    /// <summary>
    /// Return a type name to containing only the assembly and type names, not the version, culture, or PublicKeyToken.
    /// </summary>
    public static string? IsolateAssemblyAndTypeName(Type? type)
        => IsolateAssemblyAndTypeName(type?.AssemblyQualifiedName);

    /// <summary>
    /// Run the action only if the parameter is null. Useful in lambda expressions.
    /// </summary>
    public static void IfNotNull<T>(T? parameter, Action<T> action)
    {
        if (parameter is not null)
        {
            action.Invoke(parameter);
        }
    }

    public static FrozenDictionary<TTriggerKey, IncomingTriggerComposite> ComposeDevicesIntoTriggerDictionary<TTriggerKey>(
        IEnumerable<IncomingTriggerDeviceBase<TTriggerKey>> devices,
        ILoggerFactory loggerFactory)
            where TTriggerKey : struct, Enum
        => Enum.GetValues<TTriggerKey>().ToFrozenDictionary(
                static t => t,
                t => new IncomingTriggerComposite(
                    devices.Select(d => d.TriggersBase[t]),
                    t.ToString(),
                    GetTriggerParameters(t.GetType().GetField(t.ToString()))?.Name ?? "No name",
                    loggerFactory.CreateLogger($"{nameof(IncomingTriggerComposite)}({t})")
                )
            );

    internal static TriggerParameters? GetTriggerParameters(MemberInfo? enumMemberInfo)
        => enumMemberInfo?.GetCustomAttributes(typeof(TriggerParameters), false).FirstOrDefault() as TriggerParameters;

    internal static ITriggerDefaultSpecification? GetTriggerDefaultSpecification<TTriggerDevice>(MemberInfo? enumMemberInfo, int deviceInstanceIndex)
        where TTriggerDevice : IIncomingTriggerDeviceBase
        => enumMemberInfo?.GetCustomAttributes(typeof(TriggerDefaultSpecification<TTriggerDevice>))
            .OfType<TriggerDefaultSpecification<TTriggerDevice>>()
            .FirstOrDefault(s => s.ParentDeviceInstanceIndex == deviceInstanceIndex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Process timed out and was terminated")]
    private static partial void LogProcessTimedOut(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Watchdog is waiting {defaultedTimeout0} for process to end.")]
    private static partial void LogWatchdogWaiting(this ILogger logger, TimeSpan defaultedTimeout0);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception while trying to rename broken file at {path}")]
    private static partial void LogRenameBrokenFileFailed(this ILogger logger, Exception ex, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Renamed broken file to {newPath}")]
    private static partial void LogRenamedBrokenFile(this ILogger logger, string newPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Delaying by {seconds} seconds to enforce minimum uptime setting")]
    private static partial void LogDelayingForMinimumUptime(this ILogger logger, double seconds);

    [LoggerMessage(Level = LogLevel.Information, Message = "Delay complete")]
    private static partial void LogMinimumUptimeDelayComplete(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "System up time is {uptime}, so no need to wait")]
    private static partial void LogMinimumUptimeAlreadyMet(this ILogger logger, string uptime);
}
