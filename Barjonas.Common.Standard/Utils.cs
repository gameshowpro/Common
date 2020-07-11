// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using Barjonas.Common.Model;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;

namespace Barjonas.Common
{
    public static partial class Utils
    {
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
        public static string ToOrdinal(this int value, bool fromZeroBased = false)
        {
            string oneBased = ((value) + (fromZeroBased ? 1 : 0)).ToString();
            string suffix;
            switch (oneBased.Last())
            {
                case '1':
                    suffix = "st";
                    break;
                case '2':
                    suffix = "nd";
                    break;
                case '3':
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }
            return oneBased + suffix;
        }

        /// <summary>
        /// Extension method which returns the string specified in the Description attribute of an Enum, if any.  Oherwise, name is returned.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns></returns>
        public static string Description(this Enum value)
        {
            var attrs = value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs?.Any() == true)
            {
                return (attrs.First() as DescriptionAttribute).Description;
            }

            //Fallback
            return value?.ToString().Replace("_", " ");
        }

        public static object UnderlyingValue(this Enum value)
        {
            return Convert.ChangeType(value, value.GetTypeCode());
        }

        public static void StartProcessTerminateWatchdog(Logger logger = null, TimeSpan? timeout = null)
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

        private static readonly Dictionary<char, char> s_replacements = new Dictionary<char, char>()
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

        private static readonly HashSet<char> s_invalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
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

        /// <summary>
        /// Analyse the input to establish whether is is already upper-case. By default, 50% lower case characters are allowed before whole string . If it is upper case, pass back untouched.
        /// For very short 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UpperCaseIfRequired(string input, double allowedLowerPercent = .50)
        {
            if (input == null)
            {
                return null;
            }

            int lowerCount = input.Count((c) => char.IsLower(c));
            bool impossible = (1d / input.Length) > allowedLowerPercent;
            if ((impossible && (lowerCount < 1)) || (((float)lowerCount) / input.Length) > allowedLowerPercent)
            {
                return input.ToUpper();
            }
            else
            {
                return input;
            }
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
            string getPart(double n, string s) => n > 0 ? PluralIfRequired(n, s) : null;
            TimeSpan timeVal = timespan.Value;
            string[] dayParts = new[] { getPart(timeVal.Days, "day"), getPart(timeVal.Hours, "hour"), getPart(timeVal.Minutes, "minute") }
                .Where(s => s != null)
                .ToArray();

            int numberOfParts = dayParts.Length;

            switch (numberOfParts)
            {
                case 0:
                    return "a few seconds";
                case 1:
                    return dayParts[0];
                default:
                    return string.Join(", ", dayParts, 0, numberOfParts - 1) + " and " + dayParts[numberOfParts - 1];
            }                                       
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
        /// Enure the given generic list is sized within the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(ref IList<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new List<T>();
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
        /// <param name="count">The number of entries required.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(ref IList<T> list, int count, Func<int, T> factory)
        {
            EnsureListCount(ref list, count, int.MaxValue, factory);
        }

        /// <summary>
        /// Enure the given generic list contains the specified number of entries.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="count">The number of entries required.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(ref List<T> list, int count, Func<int, T> factory)
        {
            IList<T> ilist = list;
            EnsureListCount(ref ilist, count, int.MaxValue, factory);
            list = (List<T>)ilist;
        }

        /// <summary>
        /// Enure the given generic list contains the specified number of entries.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="count">The number of entries required.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(ref ObservableCollection<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new ObservableCollection<T>();
            }
            IList<T> ilist = list;
            EnsureListCount(ref ilist, minCount, maxCount, factory);
        }

        /// <summary>
        /// Enure the given generic list contains the specified number of entries.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="count">The number of entries required.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(ref ObservableCollectionEx<T> list, int minCount, int maxCount, Func<int, T> factory) where T : INotifyPropertyChanged
        {
            if (list == null)
            {
                list = new ObservableCollectionEx<T>();
            }
            IList<T> ilist = list;
            EnsureListCount(ref ilist, minCount, maxCount, factory);
        }

        /// <summary>
        /// Enure the given generic list is sized within the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(ref List<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            IList<T> ilist = list;
            EnsureListCount(ref ilist, minCount, maxCount, factory);
            list = (List<T>)ilist;
        }

        /// <summary>
        /// Enure the given binding list is sized within the specified range.
        /// </summary>
        /// <typeparam name="T">The type of the generic list.</typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="minCount">The minimum number of entries required.</param>
        /// <param name="maxCount">The maximum number of entries allowed.</param>
        /// <param name="factory">A function which will create a new list entry given the current count of the list.</param>
        public static void EnsureListCount<T>(ref BindingList<T> list, int minCount, int maxCount, Func<int, T> factory)
        {
            if (list == null)
            {
                list = new BindingList<T>();
            }
            IList<T> ilist = list;
            EnsureListCount(ref ilist, minCount, maxCount, factory);
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
                Utils.SetIndices(list);
            }
        }

        public static void Nudge<T>(this T item, int vector) where T : IIndexed, IListChild<T>
        {
            Nudge(item.Parent, item, vector);
        }


        public static bool RelaunchAsAdministrator()
        {
            ProcessStartInfo info = new ProcessStartInfo()
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
        /// <returns></returns>
        public static T Depersist<T>(string path, out bool isNew, Logger logger = null) where T : class, new()
        {
            var ser = new JsonSerializer()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };
            T obj = null;
            if (File.Exists(path))
            {
                using (var sr = new StreamReader(path))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    try
                    {
                        obj = ser.Deserialize<T>(reader);
                    }
                    catch (Exception ex)
                    {
                        logger?.Error(ex, "Exception while deserializing {0}", path);
                    }
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
        public static void Persist<T>(T obj, string path)
        {
            EnsureDirectory(path);
            var ser = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };
            using (var sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                ser.Serialize(writer, obj);
            }
        }

        /// <summary>
        /// Return the path of the NLog log file which is currently receiving events from the specified target.
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public static string CurrentNLogLogPath(string targetName = "f")
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
            string path = CurrentNLogLogPath(target);
            if (!File.Exists(path))
            {
                return false;
            }
            UriBuilder uri = new UriBuilder("vscode", "file") { Path = path + ":999999:0" };

            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = uri.Uri.AbsoluteUri,
                //Arguments = "\"" + path + "\"",
                WindowStyle = ProcessWindowStyle.Hidden
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

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += ToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += ToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                {
                    words += "and ";
                }

                if (number < 20)
                {
                    words += s_unitsMap[number];
                }
                else
                {
                    words += s_tensMap[number / 10];
                    if ((number % 10) > 0)
                    {
                        words += "-" + s_unitsMap[number % 10];
                    }
                }
            }

            return words;
        }
    }
}
