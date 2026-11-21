// (C) Barjonas LLC 2018

namespace GameshowPro.Common.AttachedProperties;

/// <summary>
/// Attached property that selects all text in a TextBox when it receives focus.
/// <remarks>Docs added by AI.</remarks>
/// </summary>
public class SelectTextOnFocus : DependencyObject
{
    /// <summary>Attached property that enables the select-on-focus behavior.</summary>
    /// <remarks>Docs added by AI.</remarks>
    public static readonly DependencyProperty s_activeProperty = DependencyProperty.RegisterAttached(
        "Active",
        typeof(bool),
        typeof(SelectTextOnFocus),
        new PropertyMetadata(false, ActivePropertyChanged));

    private static void ActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if (e.NewValue is true)
            {
                textBox.GotKeyboardFocus += OnKeyboardFocusSelectText;
                textBox.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            }
            else
            {
                textBox.GotKeyboardFocus -= OnKeyboardFocusSelectText;
                textBox.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            }
        }
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DependencyObject? dependencyObject = GetParentFromVisualTree(e.OriginalSource);

        if (dependencyObject is null)
        {
            return;
        }

        TextBox textBox = (TextBox)dependencyObject;
        if (!textBox.IsKeyboardFocusWithin)
        {
            textBox.Focus();
            e.Handled = true;
        }

        static DependencyObject? GetParentFromVisualTree(object source)
        {
            DependencyObject? parent = source as UIElement;
            while (parent is not null and not TextBox)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }
    }

    private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (e.OriginalSource is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    /// <summary>Get the Active value from the attached property.</summary>
    /// <param name="object">The dependency object.</param>
    /// <remarks>Docs added by AI.</remarks>
    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(TextBox))]
    public static bool GetActive(DependencyObject @object)
        => (bool)@object.GetValue(s_activeProperty);

    /// <summary>Set the Active value on the attached property.</summary>
    /// <param name="object">The dependency object.</param>
    /// <param name="value">True to enable select-on-focus; false to disable.</param>
    /// <remarks>Docs added by AI.</remarks>
    public static void SetActive(DependencyObject @object, bool value)
        => @object.SetValue(s_activeProperty, value);
}
