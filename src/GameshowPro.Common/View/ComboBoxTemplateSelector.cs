using System.Windows.Markup;
using System.Windows.Media;

namespace GameshowPro.Common.View;

/// <summary>
/// Usage examples:
/// <para>Specify only SelectedItemTemplate:</para>
/// <code>
/// &lt;ComboBox x:Name="MyComboBox"
///     ItemsSource="{Binding Items}"
///     ItemTemplateSelector="{comview:ComboBoxTemplateSelector SelectedItemTemplate={StaticResource MyTemplate}}" /&gt;
/// </code>
/// <para>Specify both templates:</para>
/// <code>
/// &lt;ComboBox x:Name="MyComboBox"
///     ItemsSource="{Binding Items}"
///     ItemTemplateSelector="{comview:ComboBoxTemplateSelector SelectedItemTemplate={StaticResource MySelectedItemTemplate}, DropdownItemsTemplate={StaticResource MyDropDownItemTemplate}}" /&gt;
/// </code>
/// <para>With DataTemplateSelectors:</para>
/// <code>
/// &lt;ComboBox x:Name="MyComboBox"
///     ItemsSource="{Binding Items}"
///     ItemTemplateSelector="{comview:ComboBoxTemplateSelector SelectedItemTemplateSelector={StaticResource MySelectedItemTemplateSelector}, DropdownItemsTemplateSelector={StaticResource MyDropDownItemTemplateSelector}}" /&gt;
/// </code>
/// <para>Mixed:</para>
/// <code>
/// &lt;ComboBox x:Name="MyComboBox"
///     ItemsSource="{Binding Items}"
///     ItemTemplateSelector="{comview:ComboBoxTemplateSelector SelectedItemTemplate={StaticResource MySelectedItemTemplate}, DropdownItemsTemplateSelector={StaticResource MyDropDownItemTemplateSelector}}" /&gt;
/// </code>
/// See https://stackoverflow.com/a/33421573/6671381
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class ComboBoxTemplateSelector : DataTemplateSelector
{

    public DataTemplate? SelectedItemTemplate { get; set; }
    public DataTemplateSelector? SelectedItemTemplateSelector { get; set; }
    public DataTemplate? DropdownItemsTemplate { get; set; }
    public DataTemplateSelector? DropdownItemsTemplateSelector { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {

        DependencyObject? itemToCheck = container;

        // Search up the visual tree, stopping at either a ComboBox or
        // a ComboBoxItem (or null). This will determine which template to use
        while (itemToCheck is not null
        and not ComboBox
        and not ComboBoxItem)
            itemToCheck = VisualTreeHelper.GetParent(itemToCheck);

        // If you stopped at a ComboBoxItem, you're in the dropdown
        var inDropDown = itemToCheck is ComboBoxItem;

        return inDropDown
            ? DropdownItemsTemplate ?? DropdownItemsTemplateSelector?.SelectTemplate(item, container)
            : SelectedItemTemplate ?? SelectedItemTemplateSelector?.SelectTemplate(item, container);
    }
}

public class ComboBoxTemplateSelectorExtension : MarkupExtension
{

    public DataTemplate? SelectedItemTemplate { get; set; }
    public DataTemplateSelector? SelectedItemTemplateSelector { get; set; }
    public DataTemplate? DropdownItemsTemplate { get; set; }
    public DataTemplateSelector? DropdownItemsTemplateSelector { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
        => new ComboBoxTemplateSelector()
        {
            SelectedItemTemplate = SelectedItemTemplate,
            SelectedItemTemplateSelector = SelectedItemTemplateSelector,
            DropdownItemsTemplate = DropdownItemsTemplate,
            DropdownItemsTemplateSelector = DropdownItemsTemplateSelector
        };
}
