// (C) Barjonas LLC 2018
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace GameshowPro.Common;

public static partial class Utils
{
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
        => ZipWithFill(first, firstFiller, second, secondFiller, (TFirst e1, TSecond e2) => (e1, e2));

    /// <summary>
    /// Produces a sequence of results merged elements from the two specified sequences. 
    /// If one sequence contains more elements than the other, its missing elements will be filled with the specified value.
    /// </summary>
    /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
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
    /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
    /// </summary>
    /// <param name="input">The number to be compared.</param>
    ///  <param name="min">The minimum value of the number.</param>
    ///  <param name="max">The maximum value of the number.</param>
    ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
    public static bool IsInRange(this int input, int min, int max, bool maxIsInclusive = true)
    {
        if (maxIsInclusive)
        {
            return (input <= max && input >= min);
        }
        else
        {
            return (input < max && input >= min);
        }
    }

    public static bool IsInRange(this int? input, int min, int max, bool maxIsInclusive = true)
        => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);

    /// <summary>
    /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
    /// </summary>
    /// <param name="input">The number to be compared.</param>
    ///  <param name="min">The minimum value of the number.</param>
    ///  <param name="max">The maximum value of the number.</param>
    ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
    public static bool IsInRange(this byte input, byte min, byte max, bool maxIsInclusive = true)
    {
        if (maxIsInclusive)
        {
            return (input <= max && input >= min);
        }
        else
        {
            return (input < max && input >= min);
        }
    }

    public static bool IsInRange(this byte? input, byte min, byte max, bool maxIsInclusive = true)
        => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);

    /// <summary>
    /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
    /// </summary>
    /// <param name="input">The number to be compared.</param>
    ///  <param name="min">The minimum value of the number.</param>
    ///  <param name="max">The maximum value of the number.</param>
    ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
    public static bool IsInRange(this long input, long min, long max, bool maxIsInclusive = true)
    {
        if (maxIsInclusive)
        {
            return (input <= max && input >= min);
        }
        else
        {
            return (input < max && input >= min);
        }
    }

    public static bool IsInRange(this long? input, long min, long max, bool maxIsInclusive = true)
        => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);

    /// <summary>
    /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
    /// </summary>
    /// <param name="input">The number to be compared.</param>
    ///  <param name="min">The minimum value of the number.</param>
    ///  <param name="max">The maximum value of the number.</param>
    ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
    public static bool IsInRange(this float input, float min, float max, bool maxIsInclusive = true)
    {
        if (maxIsInclusive)
        {
            return (input <= max && input >= min);
        }
        else
        {
            return (input < max && input >= min);
        }
    }

    public static bool IsInRange(this float? input, float min, float max, bool maxIsInclusive = true)
        => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);

    /// <summary>
    /// Returns a Boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
    /// </summary>
    /// <param name="input">The number to be compared.</param>
    ///  <param name="min">The minimum value of the number.</param>
    ///  <param name="max">The maximum value of the number.</param>
    ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
    public static bool IsInRange(this double input, double min, double max, bool maxIsInclusive = true)
    {
        if (maxIsInclusive)
        {
            return (input <= max && input >= min);
        }
        else
        {
            return (input < max && input >= min);
        }
    }

    public static bool IsInRange(this double? input, double min, double max, bool maxIsInclusive = true)
        => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);

    /// <summary>
    /// Returns a Boolean indicating whether the given timespan falls between the other two.  Optionally, timespans equal to the bounds can also result in a true result.
    /// </summary>
    /// <param name="input">The number to be compared.</param>
    ///  <param name="min">The minimum value of the timespan.</param>
    ///  <param name="max">The maximum value of the timespan.</param>
    ///  <param name="maxIsInclusive">Specifies whether numbers equal to the maximum should give a true result.</param>
    public static bool IsInRange(this TimeSpan input, TimeSpan min, TimeSpan max, bool maxIsInclusive = true)
    {
        if (maxIsInclusive)
        {
            return (input <= max && input >= min);
        }
        else
        {
            return (input < max && input >= min);
        }
    }

    public static bool IsInRange(this TimeSpan? input, TimeSpan min, TimeSpan max, bool maxIsInclusive = true)
        => input.HasValue && IsInRange(input.Value, min, max, maxIsInclusive);

    /// <summary>
    /// Keeps an Int32 within the given bounds, clipping it if it falls outside.
    /// </summary>
    /// <remarks>
    /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
    /// </remarks>
    /// <param name="input">The number to be kept in bounds.</param>
    ///  <param name="floor">The lowest acceptable value.</param>
    ///  <param name="ceiling">The highest acceptable value.</param>
    public static int KeepInRange(this int input, int floor = int.MinValue, int ceiling = int.MaxValue)
    {
        if (input > ceiling)
        {
            return ceiling;
        }

        if (input < floor)
        {
            return floor;
        }

        return input;
    }

    /// <summary>
    /// Keeps an Int64 within the given bounds, clipping it if it falls outside.
    /// </summary>
    /// <remarks>
    /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
    /// </remarks>
    /// <param name="input">The number to be kept in bounds.</param>
    ///  <param name="floor">The lowest acceptable value.</param>
    ///  <param name="ceiling">The highest acceptable value.</param>
    public static long KeepInRange(this long input, long floor = long.MinValue, long ceiling = long.MaxValue)
    {
        if (input > ceiling)
        {
            return ceiling;
        }

        if (input < floor)
        {
            return floor;
        }

        return input;
    }

    /// <summary>
    /// Keeps a Single within the given bounds, clipping it if it falls outside.
    /// </summary>
    /// <remarks>
    /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
    /// </remarks>
    /// <param name="input">The number to be kept in bounds.</param>
    ///  <param name="floor">The lowest acceptable value.</param>
    ///  <param name="ceiling">The highest acceptable value.</param>
    public static float KeepInRange(this float input, float floor = float.MinValue, float ceiling = float.MaxValue)
    {
        if (input > ceiling)
        {
            return ceiling;
        }

        if (input < floor || float.IsNaN(input))
        {
            return floor;
        }

        return input;
    }

    /// <summary>
    /// Keeps a double within the given bounds, clipping it if it falls outside.
    /// </summary>
    /// <remarks>
    /// <paramref name="input"/> inputs of <c>NaN</c> are mapped to <paramref name="floor"/>, ensuring that the output is always a valid number in range.
    /// </remarks>
    /// <param name="input">The number to be kept in bounds.</param>
    ///  <param name="floor">The lowest acceptable value.</param>
    ///  <param name="ceiling">The highest acceptable value.</param>
    public static double KeepInRange(this double input, double floor = double.MinValue, double ceiling = double.MaxValue)
    {
        if (input > ceiling)
        {
            return ceiling;
        }

        if (input < floor || double.IsNaN(input))
        {
            return floor;
        }

        return input;
    }

    /// <summary>
    /// Keeps a decimal within the given bounds, clipping it if it falls outside.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="input">The number to be kept in bounds.</param>
    ///  <param name="floor">The lowest acceptable value.</param>
    ///  <param name="ceiling">The highest acceptable value.</param>
    public static decimal KeepInRange(this decimal input, decimal floor = decimal.MinValue, decimal ceiling = decimal.MaxValue)
    {
        if (input > ceiling)
        {
            return ceiling;
        }

        if (input < floor)
        {
            return floor;
        }

        return input;
    }

    /// <summary>
    /// Keeps a timespan within the given bounds, clipping it if it falls outside.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="input">The number to be kept in bounds.</param>
    ///  <param name="floor">The lowest acceptable value.</param>
    ///  <param name="ceiling">The highest acceptable value.</param>
    public static TimeSpan KeepInRange(this TimeSpan input, TimeSpan? floor = null, TimeSpan? ceiling = null)
    {
        if (ceiling.HasValue && input > ceiling.Value)
        {
            return ceiling.Value;
        }

        if (floor.HasValue && input < floor.Value)
        {
            return floor.Value;
        }

        return input;
    }

    /// <summary>
    /// Narrow a <see cref="ulong"/> to an int, clamping to <see cref="int.MaxValue"/> if it's out of range.
    /// </summary>
    /// <param name="number"></param>
    public static int NarrowToIntClamped(this ulong number)
        => number > int.MaxValue ? int.MaxValue : (int)number;

    /// <summary>
    /// Narrow a <see cref="uint"/> to an int, clamping to <see cref="int.MaxValue"/> if it's out of range.
    /// </summary>
    /// <param name="number"></param>
    public static int NarrowToIntClamped(this uint number)
        => number > int.MaxValue ? int.MaxValue : (int)number;

    /// <summary>
    /// Narrow a <see cref="long"/> to an int, clamping to <see cref="int.MaxValue"/> or <see cref="int.MinValue"/> if it's out of range.
    /// </summary>
    /// <param name="number"></param>
    public static int NarrowToIntClamped(this long number)
        => (int)KeepInRange(number, int.MinValue, int.MaxValue);

    /// <summary>
    /// Calculate the factorial of the given number.
    /// </summary>
    /// <remarks>Factorials are often used to calculate the possible distinct sequences of distinct objects.</remarks>
    /// <param name="number">The number to operate upon, e.g. the number of distinct objects.</param>
    public static ulong Factorial(this ulong number)
    {
        if (number < 2)
            return 1;

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

    /// <summary>
    /// Convert an integer to a string representing the corresponding ordinal, e.g. 1st, 2nd, 3rd etc.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <param name="fromZeroBased">If true,0 is 1st otherwise 1 is 1st.</param>
    public static string ToOrdinal(this int? value, bool fromZeroBased = false)
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }
        string oneBased = ((value.Value) + (fromZeroBased ? 1 : 0)).ToString();
        string suffix = (oneBased.Last()) switch
        {
            '1' => "st",
            '2' => "nd",
            '3' => "rd",
            _ => "th",
        };
        return oneBased + suffix;
    }

    public static string ToOrdinal(this int value, bool fromZeroBased = false)
        => ToOrdinal((int?)value, fromZeroBased);

    /// <summary>
    /// Extension method which returns the string specified in the Description attribute of an Enum, if any.  Otherwise, name is returned.
    /// </summary>
    /// <param name="value">The Enum value.</param>
    /// <returns></returns>
    public static string Description(this Enum? value)
    {
        if (value != null)
        {
            object[]? attrs = value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs?.Length > 0 && attrs.First() is DescriptionAttribute attr)
            {
                return attr.Description;
            }
        }
        //Fallback
        return value?.ToString()?.Replace("_", " ") ?? "null";
    }

    public static object UnderlyingValue(this Enum value)
    {
        return Convert.ChangeType(value, value.GetTypeCode());
    }

    /// <summary>
    /// Return the supplied enum member if it is non-null and defined, otherwise return a defined default.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="defaultValue">The default value to be returned if the value is not defined.</param>
    /// <returns></returns>
    public static T EnumFallbackToDefault<T> (this T? value, T defaultValue) where T : struct, Enum
        => !value.HasValue || !Enum.IsDefined(value.Value) ? defaultValue : value.Value;

    public static void StartProcessTerminateWatchdog(Logger? logger = null, TimeSpan? timeout = null)
    {
        TimeSpan defaultedTimeout = timeout ?? TimeSpan.FromSeconds(2);
        using (var nuker = new System.Threading.Timer(
            o =>
            {
                logger?.Error("Process timed out and was terminated");
                Process.GetCurrentProcess().Kill();
            }, null, defaultedTimeout, TimeSpan.FromMilliseconds(-1)))
        {
        }
        logger?.Debug("Watchdog is waiting {0} for process to end.", defaultedTimeout);
    }

    private static readonly Dictionary<char, char> s_replacements = new()
    {
        { '\u2013', '-' }, //Em-dash
        { '\u2014', '-' }, //two em-dash
        { '\u2015', '-' }, //Horizontal bar
        { '\u2017', '_' }, //Double low line
        { '\u2018', '\'' }, //Left single quotation mark
        { '\u2019', '\'' }, //Right single quotation mark
        { '\u201a', ',' }, //Single low quotation mark
        { '\u201b', '\'' }, //Single high reversed quotation mark
        { '\u201c', '\"' }, //Left double quotation mark
        { '\u201d', '\"' }, //Right double quotation mark
        { '\u201e', '\"' }, //Double low quotation mark
        { '\u201f', '\"' }, //Double high reversed quotation mark
        { '\u2032', '\'' }, //Prime
        { '\u2033', '\"' }, //Double prime
    };
    public static string ReplaceSmartChars(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (s_replacements.TryGetValue(chars[i], out char value))
            {
                chars[i] = value;
            }
        }
        return new string(chars);
    }

    private static readonly HashSet<char> s_invalidFileNameChars = new(Path.GetInvalidFileNameChars());
    public static string MakeValidFilename(this string input)
    {
        char[]? chars = input.ToCharArray();
        bool changed = false;
        for (var i = 0; i < chars.Length; i++)
        {
            if (s_invalidFileNameChars.Contains(chars[i]))
            {
                chars[i] = '#';
                changed = true;
            }
        }
        if (changed)
        {
            return chars?.ToString() ?? string.Empty;
        }
        else
        {
            return input;
        }
    }

    private static readonly HashSet<char> s_spacesAndTabs = [' ', '\u00A0', '\t'];
    public static bool IsSpaceOrTab(this char value) => s_spacesAndTabs.Contains(value);

    public static string? UpperCaseIfRequired(string? input, double allowedLowerPercent = .50, bool upperIfUnknown = true)
    {
        if (input == null)
        {
            return null;
        }

        if (IsUpperCase(input, allowedLowerPercent) ?? upperIfUnknown)
        {
            return input.ToUpper();
        }
        else
        {
            return input;
        }
    }

    /// <summary>
    /// If the supplied string is mostly upper case, returns true. If not, returns false. May return null if there is not enough data to determine case.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="allowedLowerPercent">The percentage of lower case letters allowed before the string is considered to be upper case.</param>
    /// <returns></returns>
    public static bool? IsUpperCase(this string input, double allowedLowerPercent = .50)
    {
        if (input == null)
        {
            return null;
        }
        int upperCount = 0;
        int lowerCount = 0;
        foreach (char c in input.ToCharArray())
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
        return (((float)lowerCount) / upperLowerCount) > allowedLowerPercent;
    }

    public static string PluralIfRequired(this int number, string singularNoun, string pluralSuffix = "s")
    {
        return $"{number} {singularNoun}{(number == 1 ? "" : pluralSuffix)}";
    }

    public static string PluralIfRequired(this double number, string singularNoun, string pluralSuffix = "s")
    {
        return $"{number} {singularNoun}{(number == 1d ? "" : pluralSuffix)}";
    }

    public static string ToSentence(this TimeSpan timespan)
    {
        return ToSentence((TimeSpan?)timespan);
    }

    public static string ToSentence(this TimeSpan? timespan)
    {
        if (!timespan.HasValue)
        {
            return "never";
        }
        static string? getPart(double n, string s) => n > 0 ? PluralIfRequired(n, s) : null;
        TimeSpan timeVal = timespan.Value;
        string[] dayParts = new[] { getPart(timeVal.Days, "day"), getPart(timeVal.Hours, "hour"), getPart(timeVal.Minutes, "minute") }
            .Where(s => s != null)
            .Select(s => s!)
            .ToArray();

        int numberOfParts = dayParts.Length;

        return numberOfParts switch
        {
            0 => "a few seconds",
            1 => dayParts[0],
            _ => string.Join(", ", dayParts, 0, numberOfParts - 1) + " and " + dayParts[numberOfParts - 1],
        };
    }

    /// <summary>
    /// Convert a TimeSpan to a string with decimal places using intuitive rounding rules, unlike string.Format.
    /// Only useful when you need at least minutes, seconds and decimals.
    /// </summary>
    /// <param name="timeSpan">The timespan to be serialized.</param>
    /// <param name="includeHour">If false, the hour data will be rolled into the minutes.</param>
    /// <param name="minuteMinimumDigits">The minimum number of digits to include in the hour portion.</param>
    public static string ToString(this TimeSpan timeSpan, bool includeHour, int minuteMinimumDigits, int decimalPlaces)
    {
        StringBuilder stringBuilder = new(32);
        int minutes;
        if (includeHour)
        {
            double hours = (timeSpan.Days * 24) + timeSpan.Hours;
            stringBuilder.Append(string.Format($"{{0:D1}}", hours));
            stringBuilder.Append(':');
            minutes = timeSpan.Minutes;
        }
        else
        {
            minutes = (timeSpan.Days * 1440) + (timeSpan.Hours * 60) + timeSpan.Minutes;
        }
        double seconds = Math.Round(timeSpan.Seconds + ((double)timeSpan.Milliseconds / 1000), decimalPlaces);
        stringBuilder.Append(string.Format($"{{0:D{minuteMinimumDigits}}}:{(seconds < 10 ? "0" : "")}{{1:F{decimalPlaces}}}", minutes, seconds));
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Perform union with a param array, which may contain 0, 1 or more items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> Union<T>(this IEnumerable<T> source, params T[] items)
    {
        return source != null ? source.Union((IEnumerable<T>)items) : items;
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
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="minCount">The minimum number of entries required.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this IList<T> list, int minCount, Func<int, T> factory)
    {
        EnsureListCount(list, minCount, int.MaxValue, factory);
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
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="count">The number of entries required.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this ObservableCollection<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="count">The number of entries required.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureOrCreateListCount<T>(ref ObservableCollection<T> list, int minCount, int maxCount, Func<int, T> factory)
    {
        list ??= [];
        EnsureListCount(list, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="count">The number of entries required.</param>
    /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
    public static void EnsureListCount<T>(this ObservableCollectionEx<T> list, int minCount, int maxCount, Func<int, T> factory) where T : INotifyPropertyChanged
    {
        IList<T> ilist = list;
        EnsureListCount(ilist, minCount, maxCount, factory);
    }

    /// <summary>
    /// Ensure the given generic list contains the specified number of entries.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list to be checked.</param>
    /// <param name="count">The number of entries required.</param>
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
        if (list != null)
        {
            builder.AddRange(list);
        }
        builder.EnsureListCount(minCount, maxCount, factory);
        return builder.ToImmutable();
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
            return original.Remove(0 - diff);
        }
        else
        {
            StringBuilder sb = new(original);
            sb.Append(padding, diff);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Set the indices of all items in the given list.
    /// </summary>
    /// <typeparam name="T">The type of the generic list.</typeparam>
    /// <param name="list">The list containing the items to be indexed.</param>
    public static void SetIndices<T>(this IEnumerable<T> list) where T : IIndexed
    {
        var i = 0;
        foreach (T item in list)
        {
            item.Index = i;
            i++;
        }
    }

    /// <summary>
    /// Performs the specified action on each element of the list, making the element's positional index available to the action.
    /// </summary>
    /// <param name="action">The System.Action&lt;T&gt; delegate to perform on each element of the list.</param>
    public static void ForEachWithIndex<T>(this IEnumerable<T> list, Action<T, int> action)
    {
        int index = 0;
        foreach (T item in list)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Nudge a given item in a given list by a given number of positions.
    /// </summary>
    public static void Nudge<T>(this IList<T>? list, T? item, int vector) where T : IIndexed
    {
        if (list == null || item == null)
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

    public static void Nudge<T>(this T item, int vector) where T : IIndexed, IListChild<T>
    {
        Nudge(item?.Parent, item, vector);
    }

    /// <summary>
    /// Return the element at the given index. If it is out of range, return null.
    /// </summary>
    /// <param name="index">The index of the character within the <see cref="StringBuilder"/>.</param>
    /// <param name="source">The <see cref="StringBuilder"/> to containing the character.</param>
    public static char? ElementAtOrNull(this StringBuilder source, int index)
        => index.IsInRange(0, source.Length, false) ? source[index] : null;

    /// <summary>
    /// A custom version of <see cref="Enumerable.ElementAtOrDefault"/> for value types which will return null if the index is out of range or the source is null.
    /// </summary>
    /// <typeparam name="T">The non-nullable value type</typeparam>
    public static T? ElementAtOrNull<T>(this IEnumerable<T> source, int index) where T : struct
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

    /// <summary>
    /// A custom version of <see cref="Enumerable.FirstOrDefault"/> for value types which will return null if the source is null or empty.
    /// </summary>
    /// <typeparam name="TSource">The non-nullable value type</typeparam>
    public static TSource? FirstOrNull<TSource>(this IReadOnlyList<TSource>? source) where TSource : struct
        => source == null || source.Count == 0 ? null : source[0];

    /// <summary>
    /// A custom version of <see cref="Enumerable.FirstOrDefault"/> for value types which will return null if the source is null or empty.
    /// </summary>
    /// <typeparam name="TSource">The non-nullable value type</typeparam>
    public static TSource? LastOrNull<TSource>(this IReadOnlyList<TSource>? source) where TSource : struct
        => source == null || source.Count == 0 ? null : source[source.Count - 1];

    /// <summary>
    /// Safely filter out null values from a <see cref="IEnumerable{TSource}"/> where <see cref="TSource"/> is a nullable value type, returning a sequence of non-nullable <see cref="TSource"/> types.
    /// </summary>
    /// <typeparam name="TSource">The item type from when nullability is to be removed.</typeparam>
    /// <param name="source">The source sequence</param>
    public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
        where TSource : struct
        => source.Where(o => o.HasValue).Select(o => o!.Value);

    /// <summary>
    /// Safely filter out null values from a <see cref="IEnumerable{TSource}"/> where <see cref="TSource"/> is a nullable reference type, returning a sequence of non-nullable <see cref="TSource"/> types.
    /// </summary>
    /// <typeparam name="TSource">The item type from when nullability is to be removed.</typeparam>
    /// <param name="source">The source sequence</param>
    public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource?> source)
    => source.Where(o => o is not null).Select(o => o!);

    /// <summary>
    /// Replace any null IEnumerable<typeparamref name="T"/> with an empty Enumerable. Most useful when running a foreach loop on a nullable IEnumerable.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Nullable IEnumerable</param>
    public static IEnumerable<T> NeverNull<T>(this IEnumerable<T>? source)
    {
        return source ?? [];
    }

    /// <summary>
    /// Replace any null non-generic IEnumerable with an empty Enumerable. Most useful when running a foreach loop on a nullable IEnumerable.
    /// </summary>
    /// <param name="source">Nullable IEnumerable</param>
    public static IEnumerable NeverNull(this IEnumerable? source)
    {
        return source ?? Enumerable.Empty<object>();
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
            return Process.Start(info) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Load any Type from a JSON file. On failure, create a new one.
    /// </summary>
    /// <typeparam name="T">A reference type which has a default constructor.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    /// <param name="logger">If supplied, this will be used to log useful log messages about the depersistance operation.</param>
    /// <param name="rethrowDeserializationExceptions">If true, any deserialization exception will be rethrown. Otherwise exceptions will be logged and a new object will be returned.</param>
    /// <param name="renameFailedFiles">If true, an file which is found but cannot be deserialized will be renamed before a default object is created.</param>
    /// <returns></returns>
    public static T Depersist<T>(string? path, out bool isNew, Logger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true)
        where T : class, new()
        => Depersist<T>(path, null, out isNew, logger, rethrowDeserializationExceptions, renameFailedFiles);

    /// <summary>
    /// Load any Type from a JSON file. On failure, create a new one.
    /// </summary>
    /// <typeparam name="T">A reference type which has a default constructor.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="serializationBinder">A custom serialization binder to be used instead of automatic handling.</param>
    /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
    /// <param name="logger">If supplied, this will be used to log useful log messages about the depersistance operation.</param>
    /// <param name="rethrowDeserializationExceptions">If true, any deserialization exception will be rethrown. Otherwise exceptions will be logged and a new object will be returned.</param>
    /// <param name="renameFailedFiles">If true, an file which is found but cannot be deserialized will be renamed before a default object is created.</param>
    /// <returns></returns>
    public static T Depersist<T>(string? path, ISerializationBinder? serializationBinder, out bool isNew, Logger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true) where T : class, new()
    {
        JsonSerializer ser = new()
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            DefaultValueHandling = DefaultValueHandling.Populate
        };
        if (serializationBinder is null)
        {
            ser.TypeNameHandling = TypeNameHandling.Auto;
        }
        else
        {
            ser.TypeNameHandling = TypeNameHandling.Objects;
            ser.SerializationBinder = serializationBinder;
        }
        T? obj = null;
        if (path is not null && File.Exists(path))
        {
            bool renameBroken = false;
            using StreamReader sr = new(path);
            {
                using JsonReader reader = new JsonTextReader(sr);
                try
                {
                    obj = ser.Deserialize<T>(reader);
                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Exception while deserializing {0}", path);
                    if (renameFailedFiles)
                    {
                        renameBroken = true;
                    }
                    if (rethrowDeserializationExceptions)
                    {
                        throw new Exception($"Exception while deserializing {path}", ex);
                    }
                }
            }
            if (renameBroken)
            {
                RenameBrokenFile(path, logger);
            }
        }
        if (obj == null)
        {
            obj = new T();
            isNew = true;
            logger?.Info("Created new object because nothing could be deserialized from {0}", path);
        }
        else
        {
            logger?.Info("Successfully deserialized from {0}", path);
            isNew = false;
        }
        return obj;
    }

    private static void RenameBrokenFile(string path, Logger? logger = null)
    {
        int i = 1;
        string? dir = Path.GetDirectoryName(path);
        string pathNoExt = Path.GetFileNameWithoutExtension(path);
        string ext = Path.GetExtension(path);
        while (true)
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
                    logger?.Error(ex, "Exception while trying to rename broken file at (0)", path);
                    return;
                }
                logger?.Info("Renamed broken file to {0}", newPath);
                return;
            }
            i++;
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
                Directory.CreateDirectory(dir);
                return true;
            }
            catch
            {
            }
        }
        return false;
    }

    /// <summary>
    /// Persist any type to a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of object to be persisted.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="obj">Object to be persisted.</param>
    public static void Persist<T>(T obj, string? path, bool enumsAsStrings = false)
        => Persist(obj, null, path, enumsAsStrings);

    /// <summary>
    /// Persist any type to a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of object to be persisted.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="obj">Object to be persisted.</param>
    public static void Persist<T>(T obj, ISerializationBinderEx? serializationBinder, string? path, bool enumsAsStrings = false)
    {
        if (obj is null || path is null)
        {
            return;
        }
        EnsureDirectory(path);
        var ser = new JsonSerializer()
        {
            Formatting = Formatting.Indented,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };
        if (serializationBinder is null)
        {
            ser.TypeNameHandling = TypeNameHandling.Auto;
        }
        else
        {
            ser.TypeNameHandling = serializationBinder.TypeNameHandling ?? TypeNameHandling.Objects;
            ser.SerializationBinder = serializationBinder;
        }
        if (enumsAsStrings)
        {
            ser.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }
        using var sw = new StreamWriter(path);
        using JsonWriter writer = new JsonTextWriter(sw);
        ser.Serialize(writer, obj);
    }

    /// <summary>
    /// Return the path of the NLog log file which is currently receiving events from the specified target.
    /// </summary>
    /// <param name="targetName"></param>
    /// <returns></returns>
    public static string? CurrentNLogLogPath(string targetName = "f")
    {
        Target target = LogManager.Configuration.FindTargetByName(targetName ?? "f");
        switch (target)
        {
            case FileTarget ft:
                return FileTargetToPath(ft);
            case NLog.Targets.Wrappers.AsyncTargetWrapper atw:
                if (atw.WrappedTarget is FileTarget wft)
                {
                    return FileTargetToPath(wft);
                }
                break;
        }
        return null;
    }

    private static string FileTargetToPath(FileTarget ft)
    {
        return Path.GetFullPath(ft.FileName.Render(new LogEventInfo { TimeStamp = DateTime.Now }));
    }

    public static bool LaunchCurrentNLogLog(string target = "f")
    {
        string? path = CurrentNLogLogPath(target);
        if (!File.Exists(path))
        {
            return false;
        }
        UriBuilder uri = new("vscode", "file") { Path = path + ":999999:0" };

        ProcessStartInfo info = new()
        {
            FileName = uri.Uri.AbsoluteUri,
            //Arguments = "\"" + path + "\"",
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true,
            Verb = "open"
        };
        try
        {
            return Process.Start(info) != null;
        }
        catch
        {
            return false;
        }
    }

    //Linear interpolation
    public static double Lerp(double start, double end, double progress)
    {
        progress = progress.KeepInRange(0, 1);
        return (float)(start * (1 - progress) + end * progress);
    }

    //Scale in a similar way to Ventuz
    public static double Scale(this double input, double minIn, double maxIn, double minOut, double maxOut)
    {
        double scaled = minOut + (double)(input - minIn) / (maxIn - minIn) * (maxOut - minOut);
        return scaled.KeepInRange(Math.Min(minOut, maxOut), Math.Max(minOut, maxOut));
    }

    private static readonly string[] s_unitsMap = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"];
    private static readonly string[] s_tensMap = ["zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"];

    public static string ToWords(this int number)
    {
        if (number == 0)
        {
            return "zero";
        }

        if (number < 0)
        {
            return "minus " + ToWords(Math.Abs(number));
        }

        var words = new StringBuilder();

        if ((number / 1000000) > 0)
        {
            words.Append(ToWords(number / 1000000) + " million ");
            number %= 1000000;
        }

        if ((number / 1000) > 0)
        {
            words.Append(ToWords(number / 1000) + " thousand ");
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words.Append(ToWords(number / 100) + " hundred ");
            number %= 100;
        }

        if (number > 0)
        {
            if (words.Length > 0)
            {
                words.Append("and ");
            }

            if (number < 20)
            {
                words.Append(s_unitsMap[number]);
            }
            else
            {
                words.Append(s_tensMap[number / 10]);
                if ((number % 10) > 0)
                {
                    words.Append("-" + s_unitsMap[number % 10]);
                }
            }
        }

        return words.ToString();
    }

    public static string? EnumerableToDelimitedString(object? value, string delimiter, int offset = 0, bool includeEmptyItems = false, string nullNumberPlaceholder = "", string nullStringPlaceholder = "NullPlaceholder")
    {
        if (value == null)
        {
            return null;
        }
        StringBuilder sb = new();
        if (value is IEnumerable<bool> bools)
        {
            foreach (bool b in bools)
            {
                sb.Append(delimiter);
                sb.Append(b ? "1" : "0");
            }
        }
        else if (value is IEnumerable<bool?> nbools)
        {
            foreach (bool? b in nbools)
            {
                sb.Append(delimiter);
                sb.Append(b.HasValue ? (b.Value ? "1" : "0") : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<string> strings)
        {
            foreach (string s in strings)
            {
                if (includeEmptyItems || !string.IsNullOrWhiteSpace(s))
                {
                    sb.Append(delimiter);
                    sb.Append(s);
                }
            }
        }
        else if (value is IEnumerable<string?> nstrings)
        {
            foreach (string? s in nstrings)
            {
                if (includeEmptyItems || !string.IsNullOrWhiteSpace(s))
                {
                    sb.Append(delimiter);
                    sb.Append(s ?? nullStringPlaceholder);
                }
            }
        }
        else if (value is IEnumerable<int> ints)
        {
            foreach (int i in ints)
            {
                sb.Append(delimiter);
                sb.Append(i + offset);
            }
        }
        else if (value is IEnumerable<int?> nints)
        {
            foreach (int? i in nints)
            {
                sb.Append(delimiter);
                sb.Append(i.HasValue ? i.Value + offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<long> longs)
        {
            foreach (long i in longs)
            {
                sb.Append(delimiter);
                sb.Append(i + offset);
            }
        }
        else if (value is IEnumerable<long?> nlongs)
        {
            foreach (long? i in nlongs)
            {
                sb.Append(delimiter);
                sb.Append(i.HasValue ? i.Value + offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<uint> uints)
        {
            foreach (uint i in uints)
            {
                sb.Append(delimiter);
                sb.Append(i + (uint)offset);
            }
        }
        else if (value is IEnumerable<uint?> nuints)
        {
            foreach (uint? i in nuints)
            {
                sb.Append(delimiter);
                sb.Append(i.HasValue ? i.Value + offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<ulong> ulongs)
        {
            foreach (ulong i in ulongs)
            {
                sb.Append(delimiter);
                sb.Append(i + (ulong)offset);
            }
        }
        else if (value is IEnumerable<ulong?> nulongs)
        {
            foreach (ulong? i in nulongs)
            {
                sb.Append(delimiter);
                sb.Append(i.HasValue ? i.Value + (ulong)offset : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<IEnumerable<int>> intList)
        {
            bool first;
            foreach (IEnumerable<int> intItems in intList)
            {
                first = true;
                sb.Append(delimiter);
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
                sb.Append(delimiter);
                sb.Append(i.HasValue ? (i.Value + offset).ToString("n3") : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<float> floats)
        {
            foreach (float i in floats)
            {
                sb.Append(delimiter);
                sb.Append((i + offset).ToString("n3"));
            }
        }
        else if (value is IEnumerable<double?> ndoubles)
        {
            foreach (double? i in ndoubles)
            {
                sb.Append(delimiter);
                sb.Append(i.HasValue ? (i.Value + offset).ToString("n3") : nullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<double> doubles)
        {
            foreach (double i in doubles)
            {
                sb.Append(delimiter);
                sb.Append((i + offset).ToString("n3"));
            }
        }
        else
        {
            return null;
        }
        if (sb.Length <= delimiter.Length)
        {
            return string.Empty;
        }
        else
        {
            return sb.ToString(delimiter.Length, sb.Length - delimiter.Length);
        }
    }

    public static IEnumerable<string>? DelimitedStringToNonNullableString(string delimited, string delimiter, string nullStringPlaceholder = "?", bool exceptionOnFailure = false)
    {
        IEnumerable<string?>? result = DelimitedStringToNullableString(delimited, delimiter, nullStringPlaceholder);
        if (exceptionOnFailure && result?.Any(t => t is null) == true)
        {
            throw new Exception("String parsing failed");
        }
        return result?.Select(t => t ?? nullStringPlaceholder);
    }


    public static IEnumerable<string?>? DelimitedStringToNullableString(string delimited, string delimiter, string? nullStringPlaceholder = "?")
    {
        if (string.IsNullOrWhiteSpace(delimited))
        {
            return null;
        }
        string[] parts = delimited.Split(new string[] { delimiter }, StringSplitOptions.None);
        return parts.Select(p => p.Length > 1 && p.FirstOrDefault() == ' ' ? p.TrimStart() : p.ToString()).Select(p => string.IsNullOrEmpty(p) || p == nullStringPlaceholder ? null : p.ToString());
    }


    public static IEnumerable<T>? DelimitedStringToNonNullableType<T>(string delimited, string delimiter, int offset = 0, bool exceptionOnFailure = false)
        where T : struct
    {
        IEnumerable<T?>? result = DelimitedStringToNullableType<T>(delimited, delimiter, offset);
        if (exceptionOnFailure && result?.Any(t => t is null) == true)
        {
            throw new Exception("String parsing failed");
        }
        return result?.WhereNotNull().Select(t => t);
    }

    public static IEnumerable<T?>? DelimitedStringToNullableType<T>(string delimited, string delimiter, int offset = 0)
        where T : struct
    {
        if (string.IsNullOrWhiteSpace(delimited))
        {
            return null;
        }
        string[] parts = delimited.Split(new string[] { delimiter.Trim() }, StringSplitOptions.None);
        Type targetType = typeof(T);
        //Todo: special handling for strings, because they won't main it through constraint
        if (targetType.IsAssignableFrom(typeof(int)))
        {
            IEnumerable<int?> result = parts.Select<string, int?>(s => int.TryParse(s.Trim(), out int i) ? i - (int)offset : null);
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
            IEnumerable<ulong?> result = parts.Select<string, ulong?>(s => ulong.TryParse(s.Trim(), out ulong i) ? i - (ulong)offset : null).ToList();
            return (IEnumerable<T?>)result;
        }
        else if (targetType.IsAssignableFrom(typeof(bool)))
        {
            IEnumerable<bool?> result = parts.Select<string, bool?>(s => bool.TryParse(s.Trim(), out bool i) ? i : null);
            return (IEnumerable<T?>)result;
        }
        return null;
    }

    /// <summary>
    /// Get the build data of an assembly which has been built with a SourceRevisionID formatted in a standardized way (see remarks).
    /// </summary>
    /// <param name="assembly"></param>
    /// <remarks>Required in project file: <![CDATA[<SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("O"))</SourceRevisionId>]]>
    /// </remarks>
    public static DateTime GetBuildDate(this Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+build";

        AssemblyInformationalVersionAttribute? attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            string value = attribute.InformationalVersion;
            int index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value[(index + BuildVersionMetadataPrefix.Length)..];
                if (DateTime.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
        }

        return default;
    }

    public static bool IsMulticast(this System.Net.IPAddress address)
    {
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6Multicast;
        }
        byte[] bytes = address.GetAddressBytes();
        return bytes[0] > 223 && bytes[0] < 240;
    }

    /// <summary>
    /// A simple method to allow a task to be fired off from synchronous code which will never be seen again unless an exception needs to be handled.
    /// </summary>
    /// <param name="task">The task to be executed.</param>
    /// <param name="exceptionHandler">A delegate which will handle and exception object in case an exception is raised.</param>
    public static async void FireAndForgetSafeAsync(this Task task, Action<Exception>? exceptionHandler = null)
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

    /// <summary>
    /// Return a shuffled sequence of numbers, ensuring the first in the sequence does not equal the given number.
    /// </summary>
    /// <param name="sequenceLength">The length of the sequence.</param>
    /// <param name="rnd">The random number generator to use.  If null, use default instance.</param>
    /// <param name="disallowedStart">A number which will not be allowed in the first position.</param>
    public static int[] ShuffledNumbers(int sequenceLength, Random? rnd, int disallowedStart)
    {
        Random random = rnd ?? s_rnd;
        var shuffle = ShuffledNumbers(sequenceLength, random);
        if (shuffle[0] == disallowedStart)
        {
            int swap = random.Next(1, sequenceLength);
            shuffle[0] = shuffle[swap];
            shuffle[swap] = disallowedStart;
        }
        return shuffle;
    }

    /// <summary>
    /// Return a shuffled sequence of numbers.
    /// </summary>
    /// <param name="start">The lowest number in the sequence.</param>
    /// <param name="count">The length of the sequence.</param>
    /// <param name="rnd">The random number generator to use. If null, use default instance.</param>
    public static int[] ShuffledNumbers(int count, Random? rnd)
    {
        int[] indices = Enumerable.Range(0, count).ToArray();
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
    /// <param name="preventedResults">A <see cref="HashSet"/> return valued to be disallowed./param>
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

    private class IntArrayComparer : IEqualityComparer<int[]>
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
            IntArrayComparer comparer = new();
            ImmutableArray<int[]>.Builder builder = ImmutableArray.CreateBuilder<int[]>(maxCount);
            HashSet<int[]> uniqueVariations = new(maxCount, comparer);
            for (int i = 0; i < maxCount; i++)
            {
                while (true)
                {
                    int[] result = ShuffledNumbers(sequenceLength, rnd, uniqueVariations);
                    if (!uniqueVariations.Contains(result))
                    {
                        builder.Add(result);
                        uniqueVariations.Add(result);
                        break;
                    }
                }
            }
            return builder.ToImmutableArray();
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
        int[][] permutations = new int[sequenceLength][];
        int[] indexes = new int[sequenceLength];
        for (int i = 1; i < sequenceLength; i++)
        {
            indexes[i] = 0;
        }
        int[] items = Enumerable.Range(0, sequenceLength).ToArray();
        for (int permutationIndex = 0; permutationIndex < permutationCount; permutationIndex++)
        {
            for (int i = 1; i < sequenceLength;)
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

                    permutations[permutationIndex] = (int[])items.Clone();

                    indexes[i]++;
                    i = 1;
                }
                else
                {
                    indexes[i++] = 0;
                }
            }
        }
        
        
        return [.. permutations];

        static void Swap(ref int a, ref int b)
        {
            (b, a) = (a, b);
        }
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
        if (r >= excluding)
        {
            return r + 1;
        }
        else
        {
            return r;
        }
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
            sb.Append((char)s_rnd.Next(65,91));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Alternative to <see cref="IDictionary<TKey, TValue>.TryGetValue"/> better suited to inlining when key is value type.
    /// </summary>
    public static TValue? TryGetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey? key)
        where TKey : struct
    {
        if (key.HasValue && dictionary?.TryGetValue(key.Value, out TValue? value) == true)
        {
            return value!;
        }
        return default;
    }

    /// <summary>
    /// Alternative to <see cref="IDictionary<TKey, TValue>.TryGetValue"/> better suited to inlining when key is reference type.
    /// </summary>
    public static TValue? TryGetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey key)
    {
        if (dictionary?.TryGetValue(key, out TValue? value) == true)
        {
            return value!;
        }
        return default;
    }

    public static bool TryGetElement<TSource>(this IEnumerable<TSource> source, int index, [MaybeNullWhen(false)] out TSource element)
    {
        Debug.Assert(source != null);

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

    public static bool TryGetElement<TSource>(this IList<TSource> source, int index, [MaybeNullWhen(false)] out TSource element)
    {
        Debug.Assert(source != null);

        if (index >= 0 && source.Count > index)
        {
            element = source[index];
            return true;
        }
        element = default;
        return false;
    }

    /// <summary>
    /// Return true and output safe reference to <see cref="ToCheck"/> if it's not null.
    /// </summary>
    /// <typeparam name="T">The type of object to check.</typeparam>
    /// <param name="toCheck">The object to check.</param>
    /// <param name="valueNotNull">The null-checked version of the object</param>
    public static bool SafeNullCheck<T>(this T? toCheck, [NotNullWhen(true)] out T? valueNotNull) where T : class
    {
        if (toCheck == null)
        {
            valueNotNull = null;
            return false;
        }
        valueNotNull = toCheck; 
        return true;
    }

    /// <summary>
    /// Return true and output safe reference to <see cref="ToCheck"/> if it's not null.
    /// </summary>
    /// <typeparam name="T">The type of object to check.</typeparam>
    /// <param name="toCheck">The object to check.</param>
    /// <param name="valueNotNull">The null-checked version of the object</param>
    public static bool SafeNullCheck<T>(this T? toCheck, [NotNullWhen(true)] out T? valueNotNull) where T : struct
    {
        if (toCheck == null)
        {
            valueNotNull = null;
            return false;
        }
        valueNotNull = toCheck;
        return true;
    }

    public static T[] ArrayRepeat<T>(T value, int count)
    {
        T[] array = new T[count];
        Array.Fill(array, value);
        return array;
    }

    public static string MakePathAbsolute(this string? relativePath, Uri basePath) =>
     (relativePath == null ? basePath : new Uri(basePath, relativePath)).LocalPath;


    public static void EnforceMinimumUptime(TimeSpan minimum, Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken)
    {
        TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        TimeSpan waitTime = minimum - uptime;
        if (waitTime > TimeSpan.Zero)
        {
            logger.LogInformation("Delaying by {seconds} seconds to enforce minimum uptime setting", waitTime.TotalSeconds);
            Task.Delay(waitTime, cancellationToken).Wait(cancellationToken);
            logger.LogInformation("Delay complete");
        }
        else
        {
            logger.LogInformation("System up time is {uptime}, so no need to wait", uptime.ToSentence());
        }
    }
}

