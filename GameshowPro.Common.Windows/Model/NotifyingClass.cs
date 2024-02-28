// (C) Barjonas LLC 2024

namespace GameshowPro.Common.Model;

[JsonObject(MemberSerialization.OptIn)]
public class NotifyingClass : ObservableClass
{
    protected readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    protected readonly Action<string> NotifyPropertyChangedAction;

    public NotifyingClass()
    {
        NotifyPropertyChangedAction = new Action<string>(base.NotifyPropertyChanged);
    }

    protected override void NotifyPropertyChanged(string name)
    {
        if (_dispatcher.CheckAccess())
        {
            base.NotifyPropertyChanged(name);
        }
        else
        {
            _dispatcher.BeginInvoke(NotifyPropertyChangedAction, DispatcherPriority.DataBind, name);
        }
    }
   
}
