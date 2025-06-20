// (C) Barjonas LLC 2018

using System.Windows.Controls.Primitives;
using Size = System.Windows.Size;

namespace GameshowPro.Common.View;

//Todo: support switching column/row dominance with a DP
/// <summary>
/// UniformGrid subclass which renders columns first rather than rows first.
/// </summary>    
public class UniformGridColsFirst : UniformGrid
{
    public static readonly DependencyProperty s_columnsFirstProperty = DependencyProperty.RegisterAttached(
        "ColumnsFirst", typeof(bool), typeof(UniformGridColsFirst), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange)
    );

    public static void SetColumnsFirst(UIElement element, bool value)
    {
        element.SetValue(s_columnsFirstProperty, value);
    }

    public static bool GetColumnsFirst(UIElement element)
    {
        return (bool)element.GetValue(s_columnsFirstProperty);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (!GetColumnsFirst(this))
        {
            return base.ArrangeOverride(arrangeSize);
        }
        if (Columns <= 0 && Rows <= 0)
        {
            Rows = 2;
        }
        if (Rows == 0)
        {
            Rows = (int)Math.Ceiling((double)Children.Count / Columns);
        }
        else if (Columns == 0)
        {
            Columns = (int)Math.Ceiling((double)Children.Count / Rows);
        }

        Rect rect = new(0, 0, arrangeSize.Width / Columns, arrangeSize.Height / Rows);
        double height = rect.Height;
        double num = arrangeSize.Height - 1;
        rect.X = rect.X + rect.Width * FirstColumn;
        foreach (UIElement internalChild in InternalChildren)
        {
            internalChild.Arrange(rect);
            if (internalChild.Visibility == Visibility.Collapsed)
            {
                continue;
            }
            rect.Y += height;
            if (rect.Y < num)
            {
                continue;
            }
            rect.X += rect.Width;
            rect.Y = 0;
        }
        return arrangeSize;
    }
}
