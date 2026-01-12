// (C) Barjonas LLC 2020

using System.Windows.Media;
using Size = System.Windows.Size;

namespace GameshowPro.Common.View;

/// <summary>
/// A <see cref="Grid"/> which can be used as an ItemsPanel, as long as all the children have explicit <see cref="Grid.RowProperty"/> properties. 
/// Instead of defining <see cref="Grid.ColumnDefinitions"/> and <see cref="Grid.RowDefinitions"/>, set <see cref="Rows"/> and <see cref="Columns"/>
/// </summary>    
public class UniformGridEx : Grid
{
    public static readonly DependencyProperty s_columnMemberPathProperty =
            DependencyProperty.Register(
                    "ColumnMemberPath",
                    typeof(string),
                    typeof(ItemsControl),
                    new FrameworkPropertyMetadata(
                            string.Empty,
                            (d, e) => UpdateChildrenBinding(d, ColumnProperty, (string)e.NewValue)));

    public string ColumnMemberPath
    {
        get { return (string)GetValue(s_columnMemberPathProperty); }
        set { SetValue(s_columnMemberPathProperty, value); }
    }

    public static readonly DependencyProperty s_rowMemberPathProperty =
    DependencyProperty.Register(
            "RowMemberPath",
            typeof(string),
            typeof(ItemsControl),
            new FrameworkPropertyMetadata(
                    string.Empty,
                    (d, e) => UpdateChildrenBinding(d, RowProperty, (string)e.NewValue)));

    public string RowMemberPath
    {
        get { return (string)GetValue(s_rowMemberPathProperty); }
        set { SetValue(s_rowMemberPathProperty, value); }
    }

    public static readonly DependencyProperty s_columnSpanMemberPathProperty =
            DependencyProperty.Register(
                    "ColumnSpanMemberPath",
                    typeof(string),
                    typeof(ItemsControl),
                    new FrameworkPropertyMetadata(
                            string.Empty,
                            (d, e) => UpdateChildrenBinding(d, ColumnSpanProperty, (string)e.NewValue)));

    public string ColumnSpanMemberPath
    {
        get { return (string)GetValue(s_columnSpanMemberPathProperty); }
        set { SetValue(s_columnSpanMemberPathProperty, value); }
    }

    public static readonly DependencyProperty s_rowSpanMemberPathProperty =
        DependencyProperty.Register(
            "RowSpanMemberPath",
            typeof(string),
            typeof(ItemsControl),
            new FrameworkPropertyMetadata(
                    string.Empty,
                    (d, e) => UpdateChildrenBinding(d, RowSpanProperty, (string)e.NewValue)));

    public string RowSpanMemberPath
    {
        get { return (string)GetValue(s_rowSpanMemberPathProperty); }
        set { SetValue(s_rowSpanMemberPathProperty, value); }
    }

    private static void UpdateChildrenBinding(DependencyObject d, DependencyProperty property, string path)
    {
        if (d is UniformGridEx grid)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(grid);
            for (int i = 0; i < childrenCount; i++)
            {
                if (VisualTreeHelper.GetChild(grid, i) is FrameworkElement child)
                {
                    UpdateChildBinding(child, property, path);
                }
            }
        }
    }

    private static void UpdateChildBinding(FrameworkElement child, DependencyProperty property, string path) 
        => child.SetBinding(property, new Binding(path));

    public static readonly DependencyProperty s_columnsProperty =
        DependencyProperty.Register("Columns", typeof(int), typeof(UniformGridEx), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsArrange, OnColumnsChanged));

    public int Columns
    {
        get { return (int)GetValue(s_columnsProperty); }
        set { SetValue(s_columnsProperty, value); }
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniformGridEx instance)
        {
            Utils.EnsureListCount(instance.ColumnDefinitions, instance.Columns, instance.Columns, i => new ColumnDefinition());
        }
    }

    public static readonly DependencyProperty s_rowsProperty =
        DependencyProperty.Register("Rows", typeof(int), typeof(UniformGridEx), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsArrange, OnRowsChanged));

    public int Rows
    {
        get { return (int)GetValue(s_rowsProperty); }
        set { SetValue(s_rowsProperty, value); }
    }

    private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniformGridEx instance)
        {
            Utils.EnsureListCount(instance.RowDefinitions, instance.Rows, instance.Rows, i => new RowDefinition());
        }
    }

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        if (visualAdded is ContentPresenter pres)
        {
            UpdateChildBinding(pres, ColumnProperty, ColumnMemberPath);
            UpdateChildBinding(pres, RowProperty, RowMemberPath);
            UpdateChildBinding(pres, ColumnSpanProperty, ColumnSpanMemberPath);
            UpdateChildBinding(pres, RowSpanProperty, RowSpanMemberPath);
        }
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
       return base.ArrangeOverride(arrangeSize);
    }
}
