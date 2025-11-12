namespace GameshowPro.Common.BaseConverters;

/// <summary>
/// Returns file name or directory name from a path based on parameter "DirectoryName".
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class PathPartConverter : ICommonValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter?.ToString()?.Equals("DirectoryName", StringComparison.InvariantCultureIgnoreCase) == true)
        {
            return Path.GetDirectoryName(value?.ToString());
        }
        return Path.GetFileName(value?.ToString());
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
