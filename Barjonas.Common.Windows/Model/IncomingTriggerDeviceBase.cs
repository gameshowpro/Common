using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Barjonas.Common.Model;

/// <summary>
/// A base for <see cref="IncomingTriggerDevice{TTriggerKey, TTrigger}"/> which is common to all <see cref="IncomingTrigger"/> subclasses.
/// </summary>
public abstract class IncomingTriggerDeviceBase<TTriggerKey> : NotifyingClass, IRemoteService
    where TTriggerKey : notnull, Enum
{

    protected IncomingTriggerDeviceBase(
        string name,
        ServiceState serviceState
    )
    {
        Name = name;
        ServiceState = serviceState;
    }

    public ServiceState ServiceState { get; }
    public string Name { get; }

    /// <summary>
    /// A dictionary containing a list of all triggers belonging to this object, keyed by <see cref="TTriggerKey"/>, widely typed as <see cref="IncomingTrigger"/>.
    /// </summary>
    public abstract ImmutableDictionary<TTriggerKey, IncomingTrigger> TriggersBase { get; }
}
