// (C) Barjonas LLC 2018

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Barjonas.Common.Model;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;

#nullable enable
namespace Barjonas.Common
{
    public static partial class Utils
    {
        //
        // Summary:
        //     Returns the first element of an IList, or a default value if the sequence contains
        //     no elements.
        //     Better for an indexable collection than the IEnumerable version.
        //
        // Parameters:
        //   source:
        //     The System.IList to return the first element of.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     default(TSource) if source is empty; otherwise, the first element in source.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     source is null.
        public static TSource? FirstOrDefault<TSource>(this IReadOnlyList<TSource>? source)
            => source == null || source.Count == 0 ? default : source[0];

        /// <summary>
        /// Returns a boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
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

        /// <summary>
        /// Returns a boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
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

        /// <summary>
        /// Returns a boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
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

        /// <summary>
        /// Returns a boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
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

        /// <summary>
        /// Returns a boolean indicating whether the given number falls between the other two.  Optionally, numbers equal to the bounds can also result in a true result.
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

        /// <summary>
        /// Returns a boolean indicating whether the given timespan falls between the other two.  Optionally, timespans equal to the bounds can also result in a true result.
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
            string oneBased = ((value) + (fromZeroBased ? 1 : 0)).ToString();
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
        /// Extension method which returns the string specified in the Description attribute of an Enum, if any.  Oherwise, name is returned.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns></returns>
        public static string Description(this Enum? value)
        {
            if (value != null)
            {
                object[]? attrs = value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs?.Any() == true && attrs.First() is DescriptionAttribute attr)
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
                if (s_replacements.ContainsKey(chars[i]))
                {
                    chars[i] = s_replacements[chars[i]];
                }
            }
            return new string(chars);
        }

        private static readonly HashSet<char> s_invalidFileNameChars = new(Path.GetInvalidFileNameChars());
        public static string MakeValidFilename(this string input)
        {
            var chars = input.ToCharArray();
            var changed = false;
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
                return chars.ToString();
            }
            else
            {
                return input;
            }
        }

        private static readonly HashSet<char> s_spacesAndTabs = new() { ' ', '\u00A0', '\t' } ;
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

        public static string PluralIfRequired(this int number, string singularNoun)
        {
            return $"{number} {singularNoun}{(number == 1 ? "" : "s")}";
        }

        public static string PluralIfRequired(this double number, string singularNoun)
        {
            return $"{number} {singularNoun}{(number == 1d ? "" : "s")}";
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
        /// Peform union with a param array, which may contain 0, 1 or more items.
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
        /// Enure the given generic list is sized within the specified range, creating it if it doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureAndCreateListCount<T>(ref IList<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new List<T>();
            }
            EnsureListCount(list, minCount, maxCount, factory);
        }

        /// <summary>
        /// Enure the given generic list is sized within the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(this IList<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                throw new ArgumentNullException();
            }
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
        /// Enure the given generic list contains the specified number of entries.
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
        /// Enure the given generic list contains the specified number of entries.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureOrCreateListCount<T>(ref List<T> list, int minCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new List<T>();
            }
            EnsureListCount(list, minCount, factory);
        }

        /// <summary>
        /// Enure the given generic list contains the specified number of entries.
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
        /// Enure the given generic list contains the specified number of entries.
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
        /// Enure the given generic list contains the specified number of entries.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="count">The number of entries required.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureOrCreateListCount<T>(ref ObservableCollection<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new ObservableCollection<T>();
            }
            EnsureListCount(list, minCount, maxCount, factory);
        }

        /// <summary>
        /// Enure the given generic list contains the specified number of entries.
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
        /// Enure the given generic list contains the specified number of entries.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="count">The number of entries required.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureOrCreateListCount<T>(ref ObservableCollectionEx<T> list, int minCount, int maxCount, Func<int, T> factory) where T : INotifyPropertyChanged
        {
            if (list == null)
            {
                list = new ObservableCollectionEx<T>();
            }
            EnsureListCount(list, minCount, maxCount, factory);
        }

        /// <summary>
        /// Enure the given generic list is sized within the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureOrCreateListCount<T>(ref List<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new List<T>();
            }
            EnsureListCount(list, minCount, maxCount, factory);
        }

        /// <summary>
        /// Enure the given generic list is sized within the specified range.
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
        /// Enure the given binding list is sized within the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureOrCreateListCount<T>(ref BindingList<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new BindingList<T>();
            }
            IList<T> ilist = list;
            EnsureListCount(ilist, minCount, maxCount, factory);
        }

        /// <summary>
        /// Enure the given binding list is sized within the specified range.
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
        /// Enure the given immutable list builder is sized within the specified range.
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
        /// Enure the given immutable list is sized within the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the immutable list.</typeparam>
        /// <param name="list">The immutable list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static ImmutableList<T> EnsureListCount<T>(this ImmutableList<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                throw new ArgumentNullException();
            }
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
            int diff =  targetLength - original.Length;
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
        /// Nudge a given item in a given list by a given number of positions.
        /// </summary>
        public static void Nudge<T>(this IList<T> list, T item, int vector) where T : IIndexed
        {
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
            Nudge(item.Parent, item, vector);
        }

        /// <summary>
        /// A custom version of <see cref="Enumerable.ElementAtOrDefault"/> for value types which will return null if the index is out of range.
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
                if(index.IsInRange(0, col.Count - 1))
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
        /// Replace any null IEnumerable with an empty Enumerable. Most useful when running a foreach loop on a nullable IEnumerable.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="source">Nullable IEnumerable</param>
        public static IEnumerable<T> NeverNull<T>(this IEnumerable<T>? source)
        {
            return source ?? Enumerable.Empty<T>();
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
        /// Load any type from a JSON file. On failure, create a new one.
        /// </summary>
        /// <typeparam name="T">A reference type which has a default constructor.</typeparam>
        /// <param name="path">Path to the JSON file.</param>
        /// <param name="isNew">If an object is created (due to file not existing or being invalid) this will be set to true.</param>
        /// <param name="logger">If supplied, this will be used to log useful log messages about the depersistance operation.</param>
        /// <param name="rethrowDeserializationExceptions">If true, any deserialization exception will be rethrown. Otherwise exceptions will be logged and a new object will be returned.</param>
        /// <param name="renameFailedFiles">If true, an file which is found but cannot be deserialized will be renamed before a default object is created.</param>
        /// <returns></returns>
        public static T Depersist<T>(string path, out bool isNew, Logger? logger = null, bool rethrowDeserializationExceptions = false, bool renameFailedFiles = true) where T : class, new()
        {
            var ser = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                DefaultValueHandling = DefaultValueHandling.Populate
            };
            T? obj = null;
            if (File.Exists(path))
            {
                bool renameBroken = false;
                using var sr = new StreamReader(path);
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
                logger?.Info("Sucessfully deserialized from {0}", path);
                isNew = false;
            }
            return obj;
        }

        private static void RenameBrokenFile(string path, Logger? logger = null)
        {
            int i = 1;
            string dir = Path.GetDirectoryName(path);
            string pathNoExt = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            while (true)
            {
                string newPath = Path.Combine(dir, $"{pathNoExt}.broken.{i}{ext}");
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
                    logger?.Info("Renamed broken file to (0)", newPath);
                    return;
                }
                i++;
            }
        }

        /// <summary>
        /// Create a directory for a given path if it does not exist. Path may be to a file or directory.
        /// </summary>
        /// <param name="path">A path to a directory, or a file in a directory.</param>
        public static void EnsureDirectory(string path)
        {
            string dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// Persist any type to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of object to be persisted.</typeparam>
        /// <param name="path">Path to the JSON file.</param>
        /// <param name="obj">Object to be persisted.</param>
        public static void Persist<T>(T obj, string path, bool enumsAsStrings = false)
        {
            if (obj == null)
            {
                return;
            }
            EnsureDirectory(path);
            var ser = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };
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

        private static readonly string[] s_unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        private static readonly string[] s_tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

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

        /// <summary>
        /// Get the build data of an assembly which has been built with a SourceRevisionID formatted in a standardized way (see remarks).
        /// </summary>
        /// <param name="assembly"></param>
        /// <remarks>Required in project file: <![CDATA[<SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("O"))</SourceRevisionId>]]>
        /// </remarks>
        public static DateTime GetBuildDate(this Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            AssemblyInformationalVersionAttribute attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                string value = attribute.InformationalVersion;
                int index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
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
        /// <param name="exceptionHandler">A delagate which will handle and exception object in case an exception is raised.</param>
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
    }
}
#nullable restore
