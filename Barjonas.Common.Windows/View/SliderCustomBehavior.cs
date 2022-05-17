using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using static Barjonas.Common.Wpf.Utils;

namespace Barjonas.Common.View;

/// <summary>
/// Behaviour for slider which causes the value to be changed whenever the pointer passes across it with the mouse down.
/// This is less fiddly when quickly setting the levels of many sliders.
/// </summary>
public class SliderCustomBehavior : Behavior<Slider>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseMove += OnMouseMove;
    }


    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseMove -= OnMouseMove;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }
        AssociatedObject.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left) { RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent });
    }
}
