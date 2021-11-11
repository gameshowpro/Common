// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Barjonas.Common.Converters
{
    public enum StringConverterJoinType
    {
        Comma,
        Simple,
        Newline,
        Bullet
    }
    public class EnumerableToStringConverter : IValueConverter
    {
        private static readonly (string,int)[] s_joinTypes = new (string, int)[] { (", ", 2), ("", 0), ("\n", 2), ("\n\u2022", 1) };
        public StringConverterJoinType JoinType { get; set; } = StringConverterJoinType.Comma;
        public bool IncludeEmptyItems { get; set; } = false;
        public int IntUiOffset { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            (string seperator, int trimLength) = s_joinTypes[(int)JoinType];
            StringBuilder sb = null;
            if (value is IEnumerable<bool> bools)
            {
                sb = new StringBuilder();
                foreach (bool b in bools)
                {
                    sb.Append(seperator);
                    sb.Append(b ? "1" : "0");
                }
            }
            else if (value is IEnumerable<string> strings)
            {
                sb = new StringBuilder();
                foreach (string s in strings)
                {
                    if (IncludeEmptyItems || !string.IsNullOrWhiteSpace(s))
                    {
                        sb.Append(seperator);
                        sb.Append(s);
                    }
                }
            }
            else if(value is IEnumerable<int> ints)
            {
                sb = new StringBuilder();
                foreach (int i in ints)
                {
                    sb.Append(seperator);
                    sb.Append(i + IntUiOffset);
                }
            }
            else if (value is IEnumerable<IEnumerable<int>> intList)
            {
                sb = new StringBuilder();
                bool first;
                foreach (IEnumerable<int> intItems in intList)
                {
                    first = true;
                    sb.Append(seperator);
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
                        _ = sb.Append(i + IntUiOffset);
                    }
                }
            }
            if (sb != null)
            {
                if (trimLength > 0 && sb.Length > trimLength)
                {
                    return sb.ToString(trimLength, sb.Length - trimLength);
                }
                else
                {
                    return sb.ToString();
                }
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //todo: support all the cases supported in the other direction
            if (value is string valstr)
            {
                string[] parts = valstr.Split(',');
                if (targetType.IsAssignableFrom(typeof(List<string>)))
                {
#if NETFRAMEWORK
                    return parts.Select(p => p.FirstOrDefault() == ' ' ? p.Skip(1) : p).ToList();
#else
                    return parts.Select(p => p.FirstOrDefault() == ' ' ? p[1..] : p).ToList();
#endif
                }
                else if (targetType.IsAssignableFrom(typeof(List<int>)))
                {
                    return parts.Select(s => int.TryParse(s.Trim(), out int i) ? i - IntUiOffset : 0).ToList();
                }
                else if (targetType.IsAssignableFrom(typeof(List<bool>)))
                {
                    return parts.Select(s => bool.TryParse(s.Trim(), out bool b) && b).ToList();
                }
            }
            return Binding.DoNothing;
        }
    }
}
