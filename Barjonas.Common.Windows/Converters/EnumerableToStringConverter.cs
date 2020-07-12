// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
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
                    sb.Append(i + 1);
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
            return Binding.DoNothing;
        }
    }
}
