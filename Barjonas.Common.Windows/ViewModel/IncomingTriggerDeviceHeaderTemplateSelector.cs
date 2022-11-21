namespace Barjonas.Common.ViewModel;

internal class IncomingTriggerDeviceHeaderTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (container is FrameworkElement element)
        {
            string templateName = $"{GetTypeName(item.GetType())}.Header";

            return element.FindResource(templateName) as DataTemplate;
        }
        return null;
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            return type.Name.Split("`")[0];
        }
        return type.Name;
    }
}
