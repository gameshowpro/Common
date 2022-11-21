// (C) Barjonas LLC 2018

using System.Windows.Controls.Primitives;

namespace Barjonas.Common.View;

/// <summary>
/// This is a decorator for WPF comboboxes. It ensures that selected values survive an update of an items source, contrary to default behavior.
/// </summary>
/// <example>
///            <ComboBox 
///             SelectedValue = "{Binding SelectedCaptureDevice}" 
///             SelectedValuePath = "guid" 
///             e:ComboBoxValueHoldDecorator.ItemsSource = "{Binding CaptureDeviceList}">
///            <ComboBox>
/// </example>
public static class ComboBoxValueHoldDecorator
{
    public static readonly DependencyProperty s_itemsSourceProperty = DependencyProperty.RegisterAttached(
        "ItemsSource", typeof(IEnumerable), typeof(ComboBoxValueHoldDecorator), new PropertyMetadata(null, ItemsSourcePropertyChanged)
    );

    public static void SetItemsSource(UIElement element, IEnumerable value)
    {
        element.SetValue(s_itemsSourceProperty, value);
    }

    public static IEnumerable GetItemsSource(UIElement element)
    {
        return (IEnumerable)element.GetValue(s_itemsSourceProperty);
    }

    private static void ItemsSourcePropertyChanged(DependencyObject element,
                    DependencyPropertyChangedEventArgs e)
    {
        if (element is not Selector target)
        {
            return;
        }

        // Save original binding 
        Binding originalBinding = BindingOperations.GetBinding(target, Selector.SelectedValueProperty);
        int? selectedIndex = null;
        if (originalBinding == null)
        {
            selectedIndex = (int)element.GetValue(Selector.SelectedIndexProperty);
        }
        else
        {
            BindingOperations.ClearBinding(target, Selector.SelectedValueProperty);
        }
        try
        {
            target.ItemsSource = e.NewValue as IEnumerable;
        }
        finally
        {
            if (originalBinding == null)
            {
                if (selectedIndex.HasValue)
                {
                    object? value = ItemAt(target.ItemsSource, selectedIndex.Value);
                    if (value != null)
                    {
                        element.SetValue(Selector.SelectedIndexProperty, selectedIndex.Value);
                        element.SetValue(Selector.SelectedValueProperty, value);
                    }
                }
            }
            else
            {
                BindingOperations.SetBinding(target, Selector.SelectedValueProperty, originalBinding);
            }
        }
    }

    private static object? ItemAt(IEnumerable? list, int index)
    {
        if (list == null) return null;
        IEnumerator enumerator = list.GetEnumerator();
        if (index < 0)
        {
            return null;
        }
        for (int i = 0; i <= index; i++)
        {
            if (!enumerator.MoveNext())
            {
                return null;
            }
        }
        return enumerator.Current;
    }

    //public static int GetSelectedIndex(UIElement obj)
    //{
    //    return (int)obj.GetValue(s_selectedIndexProperty);
    //}

    //public static void SetSelectedIndex(UIElement obj, int value)
    //{
    //    obj.SetValue(s_selectedIndexProperty, value);
    //}

    //public static readonly DependencyProperty s_selectedIndexProperty =
    //    DependencyProperty.RegisterAttached("SelectedIndex", typeof(int), typeof(ComboBoxValueHoldDecorator), new PropertyMetadata(-1, SelectedIndexPropertyChanged));

    //private static void SelectedIndexPropertyChanged(DependencyObject element,
    //        DependencyPropertyChangedEventArgs e)
    //{
    //    var target = element as Selector;
    //    if (element == null)
    //    {
    //        return;
    //    }
    //    var newValue = (int)e.NewValue;
    //    if (newValue >= 0)
    //    {
    //        target.SelectedIndex = newValue;
    //    }
    //}
}
