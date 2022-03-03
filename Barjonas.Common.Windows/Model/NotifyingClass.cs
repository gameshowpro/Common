// (C) Barjonas LLC 2018

using System;
using System.ComponentModel;
using System.Windows.Threading;
using Newtonsoft.Json;
#nullable enable
namespace Barjonas.Common.Model;

[JsonObject(MemberSerialization.OptIn)]
public class NotifyingClass : ObservableClass
{
    protected readonly static Dispatcher s_dispatcher = Dispatcher.CurrentDispatcher;

    protected override void NotifyPropertyChanged(string name)
    {
        if (s_dispatcher.CheckAccess())
        {
            base.NotifyPropertyChanged(name);
        }
        else
        {
            s_dispatcher.BeginInvoke(new Action<string>((s) => base.NotifyPropertyChanged(s)), name);
        }
    }
   
}
#nullable restore
