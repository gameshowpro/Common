﻿namespace GameshowPro.Common.Model;

/// <summary>
/// Class which can bind any IncomingTrigger to any ICommand, so that the command is Executed whenever the IncomingTrigger is triggered.
/// </summary>
public class TriggerBinder(bool useDispatcher = true) : ManyToManyDictionary<ITrigger, ICommand, TriggerBinder.TriggerPair>
{
    public class TriggerPair(ITrigger trigger, ICommand command, object? commandParameter, Dispatcher? dispatcher) : Tuple<ITrigger, ICommand>(trigger, command)
    {
        private readonly object? _commandParameter = commandParameter;
        private readonly Dispatcher? _dispatcher = dispatcher;
        private readonly Action<object?> _executeDelegate = new(command.Execute);
        internal void Subscribe()
            => Item1.Triggered += _dispatcher == null ? Trigger_OnTriggeredWithoutDispatcher : Trigger_OnTriggeredWithDispatcher;

        internal void Unsubscribe()
            => Item1.Triggered -= _dispatcher == null ? Trigger_OnTriggeredWithoutDispatcher : Trigger_OnTriggeredWithDispatcher;

        private void Trigger_OnTriggeredWithoutDispatcher(object? sender, TriggerArgs e)
            => Item2.Execute(_commandParameter ?? e.Data);

        private void Trigger_OnTriggeredWithDispatcher(object? sender, TriggerArgs e)
        {
            if (_dispatcher!.CheckAccess())
            {
                Item2.Execute(_commandParameter ?? e.Data);
            }
            else
            {
                _ = _dispatcher!.BeginInvoke(_executeDelegate, _commandParameter ?? e.Data);
            }
        }
    }

    private readonly Dispatcher? _dispatcher = useDispatcher ? Dispatcher.CurrentDispatcher : null;

    public bool TryAdd(ITrigger? trigger, ICommand command, object? commandParameter)
    {
        if (trigger == null)
        {
            return false;
        }
        return TryAdd(new TriggerPair(trigger, command, commandParameter, _dispatcher));
    }

    public bool TryAdd(ITrigger? trigger, params ICommand[] commands)
    {
        if (trigger == null)
        {
            return false;
        }
        bool result = false;
        foreach (ICommand command in commands)
        {
            result = TryAdd(new TriggerPair(trigger, command, null, _dispatcher)) || result;
        }
        return result;
    }

    public bool TryAdd(ICommand command, object? parameter, params ITrigger?[] triggers)
    {
        bool result = false;
        foreach (ITrigger? trigger in triggers)
        {
            if (trigger != null)
            {
                result = TryAdd(new TriggerPair(trigger, command, parameter, _dispatcher)) || result;
            }
        }
        return result;
    }

    protected override void PairAdded(TriggerPair pair)
        => pair.Subscribe();

    protected override void PairRemoved(TriggerPair pair)
        => pair.Unsubscribe();
}
