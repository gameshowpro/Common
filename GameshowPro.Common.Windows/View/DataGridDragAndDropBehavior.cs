using static GameshowPro.Common.Wpf.Utils;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace GameshowPro.Common.View;

public class DataGridDragAndDropBehavior : Behavior<DataGrid>
{
    private object? _draggedItem;
    private bool _isEditing;
    private bool _isDragging;

    #region DragEnded
    public static readonly RoutedEvent s_dragEndedEvent =
        EventManager.RegisterRoutedEvent("DragEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DataGridDragAndDropBehavior));
    public static void AddDragEndedHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie)
        {
            uie.AddHandler(s_dragEndedEvent, handler);
        }
    }
    public static void RemoveDragEndedHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie)
        {
            uie.RemoveHandler(s_dragEndedEvent, handler);
        }
    }

    private void RaiseDragEndedEvent()
    {
        RoutedEventArgs? args = new(s_dragEndedEvent);
        AssociatedObject.RaiseEvent(args);
    }
    #endregion

    #region Popup
    public static readonly DependencyProperty s_popupProperty =
        DependencyProperty.Register("Popup", typeof(System.Windows.Controls.Primitives.Popup), typeof(DataGridDragAndDropBehavior), new PropertyMetadata(null));
    public System.Windows.Controls.Primitives.Popup? Popup
    {
        get { return (System.Windows.Controls.Primitives.Popup?)GetValue(s_popupProperty); }
        set { SetValue(s_popupProperty, value); }
    }
    #endregion

    protected override void OnAttached()
    {
        base.OnAttached();
        if (Popup != null)
        {
            Popup.PlacementTarget = AssociatedObject;
        }
        AssociatedObject.BeginningEdit += OnBeginEdit;
        AssociatedObject.CellEditEnding += OnEndEdit;
        AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.MouseMove += OnMouseMove;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.BeginningEdit -= OnBeginEdit;
        AssociatedObject.CellEditEnding -= OnEndEdit;
        AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        AssociatedObject.MouseMove -= OnMouseMove;

        Popup = null;
        _draggedItem = null;
        _isEditing = false;
        _isDragging = false;
    }

    private void OnBeginEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        _isEditing = true;
        //in case we are in the middle of a drag/drop operation, cancel it...
        if (_isDragging)
            ResetDragDrop();
    }

    private void OnEndEdit(object? sender, DataGridCellEditEndingEventArgs e)
    {
        _isEditing = false;
    }

    private void OnMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (!_isEditing && sender is UIElement senderElement)
        {
            DataGridRow? row = TryFindFromPoint<DataGridRow>(senderElement, e.GetPosition(AssociatedObject));
            if (row == null || row.IsEditing || row.DataContext == CollectionView.NewItemPlaceholder)
            {
                return;
            }
            //set flag that indicates we're capturing mouse movements
            _isDragging = true;
            _draggedItem = row.Item;
        }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _isEditing || Popup?.IsOpen != true)
        {
            return;
        }

        //get the target item
        object targetItem = AssociatedObject.SelectedItem;

        if (!ReferenceEquals(_draggedItem, targetItem) && AssociatedObject.ItemsSource is IList items)
        {
            //remove the source from the list
            (AssociatedObject.ItemsSource as IList)?.Remove(_draggedItem);
            //read this after removal in case removal caused a change
            int targetIndex = items.IndexOf(targetItem);
            //move source at the target's location
            (AssociatedObject.ItemsSource as IList)?.Insert(targetIndex, _draggedItem);

            //select the dropped item
            AssociatedObject.SelectedItem = _draggedItem;

            RaiseDragEndedEvent();
        }

        //reset
        ResetDragDrop();
    }

    private void ResetDragDrop()
    {
        _isDragging = false;
        if (Popup is not null)
        {
            Popup.IsOpen = false;
        }
        AssociatedObject.IsReadOnly = false;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
            return;

        //make sure the row under the grid is being selected
        Point position = e.GetPosition(AssociatedObject);
        DataGridRow? row = TryFindFromPoint<DataGridRow>(AssociatedObject, position);
        if (Popup != null)
        {
            Popup.DataContext = _draggedItem;
            //display the popup if it hasn't been opened yet
            if (!Popup.IsOpen)
            {
                //switch to read-only mode
                AssociatedObject.IsReadOnly = true;

                //make sure the popup is visible
                Popup.IsOpen = true;
            }
            Size popupSize = new(Popup.ActualWidth, Popup.ActualHeight);

            if (row != null)
            {
                Popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                Popup.PlacementRectangle = new Rect(row.TranslatePoint(new Point(0, -row.ActualHeight / 4), AssociatedObject), popupSize);
            }
            else
            {
                AssociatedObject.IsReadOnly = false;
                Popup.IsOpen = false;
            }
        }

        if (row != null)
        {
            AssociatedObject.SelectedItem = row.Item;
        }
    }
}
