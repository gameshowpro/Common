// (C) Barjonas LLC 2018

using System.Collections;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Barjonas.Common.View
{
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
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
            "ItemsSource", typeof(IEnumerable), typeof(ComboBoxValueHoldDecorator), new PropertyMetadata(null, ItemsSourcePropertyChanged)
        );

        public static void SetItemsSource(UIElement element, IEnumerable value)
        {
            element.SetValue(ItemsSourceProperty, value);
        }

        public static IEnumerable GetItemsSource(UIElement element)
        {
            return (IEnumerable)element.GetValue(ItemsSourceProperty);
        }

        private static void ItemsSourcePropertyChanged(DependencyObject element,
                        DependencyPropertyChangedEventArgs e)
        {
            var target = element as Selector;
            if (element == null)
            {
                return;
            }

            // Save original binding 
            Binding originalBinding = BindingOperations.GetBinding(target, Selector.SelectedValueProperty);

            BindingOperations.ClearBinding(target, Selector.SelectedValueProperty);
            try
            {
                target.ItemsSource = e.NewValue as IEnumerable;
            }
            finally
            {
                if (originalBinding != null)
                {
                    BindingOperations.SetBinding(target, Selector.SelectedValueProperty, originalBinding);
                }
            }
        }
    }
}
