﻿// (C) Barjonas LLC 2018

using System.Windows.Media;


namespace GameshowPro.Common.AttachedProperties;

public class SelectTextOnFocus : DependencyObject
{
    public static readonly DependencyProperty s_activeProperty = DependencyProperty.RegisterAttached(
        "Active",
        typeof(bool),
        typeof(SelectTextOnFocus),
        new PropertyMetadata(false, ActivePropertyChanged));

    private static void ActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((e.NewValue as bool?).GetValueOrDefault(false))
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

        if (dependencyObject == null)
        {
            return;
        }

        var textBox = (TextBox)dependencyObject;
        if (!textBox.IsKeyboardFocusWithin)
        {
            textBox.Focus();
            e.Handled = true;
        }
    }

    private static DependencyObject? GetParentFromVisualTree(object source)
    {
        DependencyObject? parent = source as UIElement;
        while (parent != null && !(parent is TextBox))
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return parent;
    }

    private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (e.OriginalSource is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(TextBox))]
    public static bool GetActive(DependencyObject @object)
    {
        return (bool)@object.GetValue(s_activeProperty);
    }

    public static void SetActive(DependencyObject @object, bool value)
    {
        @object.SetValue(s_activeProperty, value);
    }
}
