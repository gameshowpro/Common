using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Barjonas.Common.View
{
    /// <summary>
    /// Some controls, particularly the data grid, mark mousewheel events as handled, even if they don't have scrolling enabled.
    /// This behavior is a workaround, making the ScrollViewer respond to preview events, which are not swallowed.
    /// Usage looks like: <ScrollViewer comview:ScrollViewerCustomizeBehavior.UsePreviewEvents="True">
    /// </summary>
    public static class ScrollViewerCustomizeBehavior
    {
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
            if (!(d is ScrollViewer scrollViewer))
                throw new InvalidOperationException($"{nameof(ScrollViewerCustomizeBehavior)}.{nameof(s_usePreviewEventsProperty)} can only be applied to controls of type {nameof(ScrollViewer)}");
            if (e.NewValue == e.OldValue)
                return;
            if ((bool)e.NewValue)
                scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            else
                scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
