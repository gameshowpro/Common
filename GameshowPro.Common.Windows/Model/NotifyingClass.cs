// (C) Barjonas LLC 2024

namespace GameshowPro.Common.Model;

public class NotifyingClass : ObservableClass
{
    protected readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    protected readonly Action<PropertyChangedEventArgs> NotifyPropertyChangedAction;

    public NotifyingClass()
    {
        NotifyPropertyChangedAction = new (NotifyPropertyChanged);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        if (_dispatcher.CheckAccess())
        {
            NotifyPropertyChanged(args);
        }
        else
        {
            _dispatcher.BeginInvoke(NotifyPropertyChangedAction, DispatcherPriority.DataBind, args);
        }
    }
   
}
