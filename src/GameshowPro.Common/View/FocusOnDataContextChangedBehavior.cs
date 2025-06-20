namespace GameshowPro.Common.View;

/// <summary>
/// Behavior which will set keyboard focus to the associated object whenever its data context changes.
/// </summary>
public class FocusOnDataContextChangedBehavior : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
    }

    private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Keyboard.Focus(AssociatedObject);
        if (AlsoClear && AssociatedObject is TextBox tb)
        {
            tb.Clear();
        }
    }

    public bool AlsoClear
    {
        get { return (bool)GetValue(s_alsoClearProperty); }
        set { SetValue(s_alsoClearProperty, value); }
    }

    // Using a DependencyProperty as the backing store for AlsoClear.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty s_alsoClearProperty =
        DependencyProperty.Register(nameof(AlsoClear), typeof(bool), typeof(FocusOnDataContextChangedBehavior), new PropertyMetadata(false));

}
