﻿namespace GameshowPro.Common.View;

public class IncomingTriggersFilterBehavior : Behavior<CollectionViewSource>
{
    private CollectionViewSource? _source;
    protected override void OnAttached()
    {
        base.OnAttached();
        _source = AssociatedObject;
        _source.Filter += AssociatedObjectOnFilter;
    }

    private void AssociatedObjectOnFilter(object sender, FilterEventArgs filterEventArgs)
    {
        if (filterEventArgs.Item is IncomingTrigger trigger)
        {
            filterEventArgs.Accepted = ShowAll || trigger.Setting.IsEnabled;
        }
    }

    private static void ShowAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IncomingTriggersFilterBehavior filterBehavior)
        {
            filterBehavior?._source?.View.Refresh();
        }
    }

    public static readonly DependencyProperty s_showAll = DependencyProperty.Register(nameof(ShowAll), typeof(bool), typeof(IncomingTriggersFilterBehavior), new PropertyMetadata(false, ShowAllChanged));
    public bool ShowAll
    {
        get { return (bool)GetValue(s_showAll); }
        set { SetValue(s_showAll, value); }
    }
}
