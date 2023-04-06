namespace Barjonas.Common.View;

/// <summary>
/// Behavior for <see cref="TextBox"/> which will catch any enter key presses and automatically update the source of any binding to the <see cref="TextBox.Text"/> property.
/// </summary>
public class UpdateSourceOnEnterBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.KeyDown += AssociatedObject_KeyDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
    }

    private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            DependencyProperty prop = TextBox.TextProperty;
            BindingExpressionBase binding = BindingOperations.GetBindingExpression(AssociatedObject, prop);
            binding ??= BindingOperations.GetMultiBindingExpression(AssociatedObject, prop);

            if (binding != null)
            {
                object value = AssociatedObject.GetValue(prop);
                if (string.IsNullOrEmpty(value.ToString()) == true)
                {
                    binding.UpdateTarget();
                }
                else
                {
                    binding.UpdateSource();
                }
                e.Handled = true;
            }
        }
    }
}
