namespace GameshowPro.Common.ViewModel;

internal class IncomingTriggerDeviceHeaderTemplateSelector : DataTemplateSelector
{
    private static readonly DataTemplate s_empty = new();

    /// <summary>The part of the resource key after the type name, so the key looked up is {type name}.{Suffix}.</summary>
    public string Suffix { get; set; } = "Header";

    /// <summary>
    /// When true, a missing template is not an error: an empty template is returned instead (null would make the
    /// ContentControl show the item's ToString()).
    /// </summary>
    public bool IsOptional { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (container is FrameworkElement element)
        {
            string templateName = $"{GetTypeName(item.GetType())}.{Suffix}";

            if (IsOptional)
            {
                return element.TryFindResource(templateName) as DataTemplate ?? s_empty;
            }
            return element.FindResource(templateName) as DataTemplate;
        }
        return null;

        static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                return type.Name.Split("`")[0];
            }
            return type.Name;
        }
    }
}
