using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
#nullable enable
namespace Barjonas.Common.Model
{
    /// <summary>
    /// Class which can bind any IncomingTrigger to any ICommand, so that the command is Executed whenever the IncomingTrigger is triggered.
    /// </summary>
    public class TriggerBinder : ManyToManyDictionary<ITrigger, ICommand, TriggerBinder.TriggerPair>
    {
        public class TriggerPair : Tuple<ITrigger, ICommand>
        {
            public TriggerPair(ITrigger trigger, ICommand command, object? commandParameter, Dispatcher? dispatcher) : base(trigger, command)
            {
                _commandParameter = commandParameter;
                _dispatcher = dispatcher;
                _executeDelegate = new(command.Execute);
            }
            private readonly object? _commandParameter;
            private readonly Dispatcher? _dispatcher;
            private readonly Action<object?> _executeDelegate;
            internal void Subscribe()
                => Item1.Triggered += _dispatcher == null ? Trigger_OnTriggeredWithoutDispatcher : Trigger_OnTriggeredWithDispatcher;

            internal void Unsubscribe()
                => Item1.Triggered -= Trigger_OnTriggeredWithoutDispatcher;

            private void Trigger_OnTriggeredWithoutDispatcher(object? sender, TriggerArgs e)
                => Item2.Execute(e.Data);

            private void Trigger_OnTriggeredWithDispatcher(object? sender, TriggerArgs e)
            {
                if (_dispatcher!.CheckAccess())
                {
                    Item2.Execute(e.Data);
                }
                else
                {
                    _ = _dispatcher!.BeginInvoke(_executeDelegate, e.Data);
                }
            }
        }

        private readonly Dispatcher? _dispatcher;

        public TriggerBinder(bool useDispatcher = true)
        {
            _dispatcher = useDispatcher ? Dispatcher.CurrentDispatcher : null;
        }

        public bool TryAdd(ITrigger trigger, ICommand command, object? commandParameter)
            => TryAdd(new TriggerPair(trigger, command, commandParameter, _dispatcher));

        public bool TryAdd(ITrigger trigger, params ICommand[] commands)
        {
            bool result = false;
            foreach (ICommand command in commands)
            {
                result = TryAdd(new TriggerPair(trigger, command, null, _dispatcher)) || result;
            }
            return result;
        }

        public bool TryAdd(ICommand command, params ITrigger[] triggers)
        {
            bool result = false;
            foreach (ITrigger trigger in triggers)
            {
                result = TryAdd(new TriggerPair(trigger, command, null, _dispatcher)) || result;
            }
            return result;
        }

        protected override void PairAdded(TriggerPair pair)
            => pair.Subscribe();

        protected override void PairRemoved(TriggerPair pair)
            => pair.Unsubscribe();
    }
}
#nullable restore
