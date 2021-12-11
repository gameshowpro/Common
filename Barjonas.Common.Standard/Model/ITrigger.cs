using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
/// <summary>
/// Implemented by objects that can raise a trigger event.
/// Allows binding to an ICommand and any other generalized code.
/// </summary>
namespace Barjonas.Common.Model
{
    public interface ITrigger
    {
        public event EventHandler<TriggerArgs> Triggered;
    }

    public class TriggerArgs : EventArgs
    {
        public TriggerArgs(object? data = null)
        {
            Data = data;
        }

        public object? Data { get; }
    }
}
#nullable restore
