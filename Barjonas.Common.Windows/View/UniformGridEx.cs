// (C) Barjonas LLC 2020

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Controls;
using Barjonas.Common;
using System.Windows.Media;
using System.ComponentModel;

namespace Barjonas.Common.View
{
    /// <summary>
    /// A <see cref="Grid"/> which can be used as an ItemsPanel, as long as all the children have explicit <see cref="Grid.RowProperty"/> properties. 
    /// Instead of defining <see cref="Grid.ColumnDefinitions"/> and <see cref="Grid.RowDefinitions"/>, set <see cref="UniformGridEx.Rows"/> and <see cref="UniformGridEx.Columns"/>
    /// </summary>    
    public class UniformGridEx : Grid
    {
        public static readonly DependencyProperty ColumnMemberPathProperty =
                DependencyProperty.Register(
                        "ColumnMemberPath",
                        typeof(string),
                        typeof(ItemsControl),
                        new FrameworkPropertyMetadata(
                                string.Empty,
                                (d, e) => UpdateChildrenBinding(d, ColumnProperty, (string)e.NewValue)));

        public string ColumnMemberPath
        {
            get { return (string)GetValue(ColumnMemberPathProperty); }
            set { SetValue(ColumnMemberPathProperty, value); }
        }

        public static readonly DependencyProperty RowMemberPathProperty =
        DependencyProperty.Register(
                "RowMemberPath",
                typeof(string),
                typeof(ItemsControl),
                new FrameworkPropertyMetadata(
                        string.Empty,
                        (d, e) => UpdateChildrenBinding(d, RowProperty, (string)e.NewValue)));

        public string RowMemberPath
        {
            get { return (string)GetValue(RowMemberPathProperty); }
            set { SetValue(RowMemberPathProperty, value); }
        }

        public static readonly DependencyProperty ColumnSpanMemberPathProperty =
                DependencyProperty.Register(
                        "ColumnSpanMemberPath",
                        typeof(string),
                        typeof(ItemsControl),
                        new FrameworkPropertyMetadata(
                                string.Empty,
                                (d, e) => UpdateChildrenBinding(d, ColumnSpanProperty, (string)e.NewValue)));

        public string ColumnSpanMemberPath
        {
            get { return (string)GetValue(ColumnSpanMemberPathProperty); }
            set { SetValue(ColumnSpanMemberPathProperty, value); }
        }

        public static readonly DependencyProperty RowSpanMemberPathProperty =
            DependencyProperty.Register(
                "RowSpanMemberPath",
                typeof(string),
                typeof(ItemsControl),
                new FrameworkPropertyMetadata(
                        string.Empty,
                        (d, e) => UpdateChildrenBinding(d, RowSpanProperty, (string)e.NewValue)));

        public string RowSpanMemberPath
        {
            get { return (string)GetValue(RowSpanMemberPathProperty); }
            set { SetValue(RowSpanMemberPathProperty, value); }
        }

        private static void UpdateChildrenBinding(DependencyObject d, DependencyProperty property, string path)
        {
            if (d is UniformGridEx grid)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(grid);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(grid, i) as FrameworkElement;
                    UpdateChildBinding(child, property, path);
                }
            }
        }

        private static void UpdateChildBinding(FrameworkElement child, DependencyProperty property, string path) 
            => child.SetBinding(property, new Binding(path));

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(UniformGridEx), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsArrange, OnColumnsChanged));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UniformGridEx instance)
            {
                Utils.EnsureListCount(instance.ColumnDefinitions, instance.Columns, instance.Columns, i => new ColumnDefinition());
            }
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(UniformGridEx), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsArrange, OnRowsChanged));

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
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
}
