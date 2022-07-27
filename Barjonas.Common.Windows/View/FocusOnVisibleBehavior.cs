using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Barjonas.Common.View
{
    /// <summary>
    /// Behavior which will set keyboard focus to the associated object whenever it becomes visible.
    /// </summary>
    public class FocusOnVisibleBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged += AssociatedObject_IsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.IsVisibleChanged -= AssociatedObject_IsVisibleChanged;
        }

        private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject.IsVisible)
            {
                Keyboard.Focus(AssociatedObject);
            }
        } 
    }
}
