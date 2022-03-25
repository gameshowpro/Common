// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
#nullable enable
namespace Barjonas.Common.Converters;

public enum StringConverterJoinType
{
    Comma,
    Simple,
    Newline,
    Bullet
}
public class EnumerableToStringConverter : IValueConverter
{
    private const string NullStringPlaceholder = "NullPlaceholder";
    private const char NullNumberPlaceholder = '?';
    private static readonly (string,int)[] s_joinTypes = new (string, int)[] { (", ", 2), ("", 0), ("\n", 2), ("\n\u2022", 1) };
    public StringConverterJoinType JoinType { get; set; } = StringConverterJoinType.Comma;
    public bool IncludeEmptyItems { get; set; } = false;
    public int IntUiOffset { get; set; } = 1;

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        (string seperator, int trimLength) = s_joinTypes[(int)JoinType];
        StringBuilder sb = new();
        if (value is IEnumerable<bool> bools)
        {
            foreach (bool b in bools)
            {
                sb.Append(seperator);
                sb.Append(b ? "1" : "0");
            }
        }
        else if (value is IEnumerable<bool?> nbools)
        {
            foreach (bool? b in nbools)
            {
                sb.Append(seperator);
                sb.Append(b.HasValue ? (b.Value ? "1" : "0")  : NullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<string> strings)
        {
            foreach (string s in strings)
            {
                if (IncludeEmptyItems || !string.IsNullOrWhiteSpace(s))
                {
                    sb.Append(seperator);
                    sb.Append(s);
                }
            }
        }
        else if (value is IEnumerable<string?> nstrings)
        {
            foreach (string? s in nstrings)
            {
                if (IncludeEmptyItems || !string.IsNullOrWhiteSpace(s))
                {
                    sb.Append(seperator);
                    sb.Append(s ?? NullStringPlaceholder);
                }
            }
        }
        else if (value is IEnumerable<int> ints)
        {
            foreach (int i in ints)
            {
                sb.Append(seperator);
                sb.Append(i + IntUiOffset);
            }
        }
        else if (value is IEnumerable<int?> nints)
        {
            foreach (int? i in nints)
            {
                sb.Append(seperator);
                sb.Append(i.HasValue ? i.Value + IntUiOffset : NullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<long> longs)
        {
            foreach (long i in longs)
            {
                sb.Append(seperator);
                sb.Append(i + IntUiOffset);
            }
        }
        else if (value is IEnumerable<long?> nlongs)
        {
            foreach (long? i in nlongs)
            {
                sb.Append(seperator);
                sb.Append(i.HasValue ? i.Value + IntUiOffset : NullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<uint> uints)
        {
            foreach (uint i in uints)
            {
                sb.Append(seperator);
                sb.Append(i + (uint)IntUiOffset);
            }
        }
        else if (value is IEnumerable<uint?> nuints)
        {
            foreach (uint? i in nuints)
            {
                sb.Append(seperator);
                sb.Append(i.HasValue ? i.Value + IntUiOffset : NullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<ulong> ulongs)
        {
            foreach (ulong i in ulongs)
            {
                sb.Append(seperator);
                sb.Append(i + (ulong)IntUiOffset);
            }
        }
        else if (value is IEnumerable<ulong?> nulongs)
        {
            foreach (ulong? i in nulongs)
            {
                sb.Append(seperator);
                sb.Append(i.HasValue ? i.Value + (ulong)IntUiOffset : NullNumberPlaceholder);
            }
        }
        else if (value is IEnumerable<IEnumerable<int>> intList)
        {
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
        else
        {
            return Binding.DoNothing;
        }
        if (trimLength > 0 && sb.Length > trimLength)
        {
            return sb.ToString(trimLength, sb.Length - trimLength);
        }
        else
        {
            return sb.ToString();
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        //todo: support all the cases supported in the other direction
        if (value is string valstr)
        {
            string[] parts = valstr.Split(',');
            if (targetType.IsAssignableFrom(typeof(List<string?>)))
            {
#if NETFRAMEWORK
                return parts.Select(p => p.FirstOrDefault() == ' ' ? p.Skip(1) : p).Select(p => (string)p == NullStringPlaceholder ? null : p).ToList();
#else
                return parts.Select(p => p.FirstOrDefault() == ' ' ? p[1..] : p).Select(p => p == NullStringPlaceholder ? null : p).ToList();
#endif
            }
            else if (targetType.IsAssignableFrom(typeof(List<string>)))
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
            else if (targetType.IsAssignableFrom(typeof(List<int?>)))
            {
                List<int?> result = parts.Select<string, int?>(s => int.TryParse(s.Trim(), out int i) ? i - (int)IntUiOffset : null).ToList();
                return result?.ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<uint>)))
            {
                return parts.Select(s => uint.TryParse(s.Trim(), out uint i) ? i - IntUiOffset : 0).ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<uint?>)))
            {
                List<uint?> result = parts.Select<string, uint?>(s => uint.TryParse(s.Trim(), out uint i) ? i - (uint)IntUiOffset : null).ToList();
                return result?.ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<long>)))
            {
                return parts.Select(s => long.TryParse(s.Trim(), out long i) ? i - IntUiOffset : 0).ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<long?>)))
            {
                List<long?> result = parts.Select<string, long?>(s => long.TryParse(s.Trim(), out long i) ? i - (long)IntUiOffset : null).ToList();
                return result?.ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<ulong>)))
            {
                return parts.Select(s => ulong.TryParse(s.Trim(), out ulong i) ? i - (ulong)IntUiOffset : 0).ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<ulong?>)))
            {
                List<ulong?> result = parts.Select<string, ulong?>(s => ulong.TryParse(s.Trim(), out ulong i) ? i - (ulong)IntUiOffset : null).ToList();
                return result?.ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<bool>)))
            {
                return parts.Select(s => bool.TryParse(s.Trim(), out bool b) && b).ToList();
            }
            else if (targetType.IsAssignableFrom(typeof(List<bool?>)))
            {
                List<bool?> result = parts.Select<string, bool?>(s => bool.TryParse(s.Trim(), out bool i) ? i : null).ToList();
                return result?.ToList();
            }
        }
        return Binding.DoNothing;
    }
}
#nullable restore
