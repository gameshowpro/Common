// (C) Barjonas LLC 2018

using System.Windows.Markup;

namespace GameshowPro.Common.Converters;

/// <summary>
/// Converts an enum Type or sequence of enum values to a list of items for binding.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class EnumToObjectArray : MarkupExtension, IValueConverter
{
    /// <summary>Optional binding to an enum value source when used as a markup extension.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public BindingBase? SourceEnum { get; set; }

    private readonly Type? _type;

    /// <summary>Create a converter instance.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public EnumToObjectArray()
    {
    }

    /// <summary>Create a converter for a specific enum type.</summary>
    /// <param name="type">The enum Type to convert.</param>
    /// <remarks>Docs added by AI.</remarks>
    public EnumToObjectArray(Type type)
    {
        _type = type;
    }

    /// <inheritdoc/>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        Type typeToUse;
        if (_type == null)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target && target.TargetObject is DependencyObject targetObject && target.TargetProperty is DependencyProperty targetProperty)
            {
                BindingOperations.SetBinding(targetObject, s_sourceEnumBindingSinkProperty, SourceEnum);
                typeToUse = targetObject.GetValue(s_sourceEnumBindingSinkProperty).GetType();
                if (typeToUse.BaseType != typeof(Enum))
                {
                    return null;
                }
            }
            else
            {
                return this;
            }
        }
        else
        {
            typeToUse = _type;
        }

        return TypeToList(typeToUse);
    }

    private static object? TypeToList(Type type)
    {
        if (type == null)
        {
            return null;
        }
        return Enum.GetValues(type)
                        .Cast<Enum>()
                        .Select(e => new { Value = e, Name = e.ToString(), DisplayName = e.Description(), Underlying = e.UnderlyingValue() });
    }

    /// <inheritdoc/>
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Type type)
        {
            //presume enum type
            return TypeToList(type);
        }
        else if (value is IEnumerable enumerable)
        { 
            //presume IEnumerable<Enum>
            return enumerable.Cast<Enum>().Select(e => new { Value = e, Name = e.ToString(), DisplayName = e.Description(), Underlying = e.UnderlyingValue() });
        }
        return null;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static readonly DependencyProperty s_sourceEnumBindingSinkProperty = DependencyProperty.RegisterAttached("SourceEnumBindingSink", typeof(Enum)
                       , typeof(EnumToObjectArray), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
}
