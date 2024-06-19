namespace GameshowPro.Common.View;

/// <summary>
/// Some controls, particularly the data grid, mark mousewheel events as handled, even if they don't have scrolling enabled.
/// This behavior is a workaround, making the ScrollViewer respond to preview events, which are not swallowed.
/// Usage looks like: <ScrollViewer comview:ScrollViewerCustomizeBehavior.UsePreviewEvents="True">
/// </summary>
public static class ScrollViewerCustomizeBehavior
{
    #region PreviewEvents
    public static bool GetUsePreviewEvents(ScrollViewer obj)
    {
        return (bool)obj.GetValue(s_usePreviewEventsProperty);
    }

    public static void SetUsePreviewEvents(ScrollViewer obj, bool value)
    {
        obj.SetValue(s_usePreviewEventsProperty, value);
    }

    public static readonly DependencyProperty s_usePreviewEventsProperty =
        DependencyProperty.RegisterAttached("UsePreviewEvents", typeof(bool), typeof(ScrollViewerCustomizeBehavior), new PropertyMetadata(false, OnPropertyChanged));

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer)
        {
            throw new InvalidOperationException($"{nameof(ScrollViewerCustomizeBehavior)}.{nameof(s_usePreviewEventsProperty)} can only be applied to controls of type {nameof(ScrollViewer)}");
        }
        if (e.NewValue == e.OldValue)
        {
            return;
        }
        if ((bool)e.NewValue)
        {
            scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
        }
        else
        {
            scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
        }
    }

    private static void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }
    #endregion

    #region BindableVerticalOffset
    /// <summary>
    /// VerticalOffset attached property
    /// </summary>
    public static readonly DependencyProperty s_verticalOffsetProperty = 
        DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerCustomizeBehavior), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVerticalOffsetPropertyChanged));

    /// <summary>
    /// Just a flag that the binding has been applied.
    /// </summary>
    private static readonly DependencyProperty s_verticalScrollBindingProperty =
        DependencyProperty.RegisterAttached("VerticalScrollBinding", typeof(double?), typeof(ScrollViewerCustomizeBehavior));

    public static double GetVerticalOffset(DependencyObject depObj)
    {
        return (double)depObj.GetValue(s_verticalOffsetProperty);
    }

    public static void SetVerticalOffset(DependencyObject depObj, double value)
    {
        depObj.SetValue(s_verticalOffsetProperty, value);
    }

    private static void OnVerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //Our dependency property was changed by one side or another
        if (d is ScrollViewer scrollViewer)
        {
            BindVerticalOffset(scrollViewer);
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }

    /// <summary>
    /// Start handling ScrollChange, if we're not already
    /// </summary>
    private static void BindVerticalOffset(ScrollViewer scrollViewer)
    {
        if (scrollViewer.GetValue(s_verticalScrollBindingProperty) == null)
        {
            scrollViewer.SetValue(s_verticalScrollBindingProperty, scrollViewer.VerticalOffset);
            scrollViewer.ScrollChanged += (s, se) =>
            {
                var sv = (ScrollViewer)s;
                bool atDefault = se.VerticalOffset == (double)sv.GetValue(s_verticalScrollBindingProperty);
                if (atDefault || se.ExtentHeightChange != 0)
                {
                    //presumably the scrollviewer just had its contents loaded, maybe due to parent tab coming into view
                    sv.ScrollToVerticalOffset(GetVerticalOffset(sv));
                }
                else if (se.VerticalChange != 0)
                {
                    //presumably the user changed the scroll value, rather than contents loading or changing
                    SetVerticalOffset(sv, se.VerticalOffset);
                }
            };
        }
    }
    #endregion
}
