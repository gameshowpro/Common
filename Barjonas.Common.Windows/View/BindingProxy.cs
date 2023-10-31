namespace Barjonas.Common.View;
/// <summary>
/// An object which, when instantiated in XAML as the earlier sibling of a <see cref="FrameworkElement"/>, can serve as proxy to the Visual Tree.
/// This is a solution to binding objects which are outside the VisualTree through a data context. E.g. the <see cref="DataGridColumn.Visibility"/> of a <see cref="DataGridColumn"/>.
/// </summary>
public class BindingProxy : Freezable
{
    #region Overrides of Freezable

    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }

    #endregion

    public object Data
    {
        get { return GetValue(s_dataProperty); }
        set { SetValue(s_dataProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty s_dataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
}
