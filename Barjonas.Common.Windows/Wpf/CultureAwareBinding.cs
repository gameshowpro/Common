namespace Barjonas.Common.Wpf;

/// <summary>
/// By default, WPF does not use the current culture for converters, including StringFormat. 
/// Using this <see cref="Binding"/> subclass is easier than customizing every converter to force the current system culture.
/// Other partial solutions will result in conversions ignoring any customization the user as configured in Windows, e.g. en-US with an ISO date format.
/// </summary>
public class CultureAwareBinding : Binding
{
    public CultureAwareBinding() : base()
    {
        ConverterCulture = CultureInfo.CurrentCulture;
    }

    public CultureAwareBinding(string path) : base(path)
    {
        ConverterCulture = CultureInfo.CurrentCulture;
    }
}
