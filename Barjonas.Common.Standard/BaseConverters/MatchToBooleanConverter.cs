// (C) Barjonas LLC 2018

namespace Barjonas.Common.BaseConverters;

/// <summary>
/// Return a visibility or boolean depending on match to one or more parameters.
/// </summary>
public abstract class MatchToBooleanConverter(object doNothing) : ICommonValueConverter, ICommonMultiValueConverter
{
    private readonly object _doNothing = doNothing;

    /// <summary>
    /// Return a <see cref="bool"/> or equivalent depending on whether the first element in <paramref name="value"/> equal the second element.
    /// </summary>
    /// <param name="values">An array containing two values, each supplied from a binding</param>
    /// <param name="targetType">The type to return. Either <see cref="bool"/> or equivalent visibility</param>
    /// <param name="parameter">Ignored</param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is int value0Int && values[1] is int value1Int) //Special case to force enum to be treated as int to allow comparison with another int
        {
            return BooleanToType(value0Int == value1Int, targetType);
        }
        return BooleanToType((values[0] == null && values[1] == null) || values[0]?.Equals(values[1]) == true, targetType);
    }

    /// <summary>
    /// Return a <see cref="bool"/> or equivalent depending on whether <paramref name="value"/> matches the <paramref name="parameter"/>.
    /// If <paramref name="parameter"/> is an array, return a bool depending on whether any of its elements equal <paramref name="value"/> 
    /// </summary>
    /// <param name="value">The value supplied by a binding.</param>
    /// <param name="targetType">The type to return. Either <see cref="bool"/> or equivalent visibility</param>
    /// <param name="parameter">The value to compare to or an array of values to compare to</param>
    /// <returns></returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BooleanToType(GetRawResult(value, parameter), targetType);
        bool GetRawResult(object? value, object? parameter)
            =>
            (parameter is IEnumerable paramList && AnyElementMatches(value, paramList)) 
            ||
            parameter switch
            {
                Type t => t.IsAssignableFrom(value?.GetType()),
                _ => (value == null && parameter == null) || value?.Equals(parameter) == true
            };
        bool AnyElementMatches(object? value, IEnumerable paramList)
        {
            foreach (object item in paramList)
            {
                if (GetRawResult(value, item))
                {
                    return true;
                }
            }
            return false;
        }
    }

    protected abstract object BooleanToType(bool value, Type type);

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        return [_doNothing, _doNothing];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(bool))
        {
            if (value == null)
            {
                return parameter == null;
            }
            return value.Equals(parameter);
        }
        return _doNothing;
    }
}
