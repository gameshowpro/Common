namespace GameshowPro.Common.BaseConverters;

public class PathPartConverter : ICommonValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter?.ToString()?.Equals("DirectoryName", StringComparison.InvariantCultureIgnoreCase) == true)
        {
            return Path.GetDirectoryName(value?.ToString());
        }
        return Path.GetFileName(value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
