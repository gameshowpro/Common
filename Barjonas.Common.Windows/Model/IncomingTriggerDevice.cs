// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using static Barjonas.Common.Model.KioskWindowHandler;
#nullable enable
namespace Barjonas.Common.Model;

/// <summary>
/// A base implementation of a trigger device which includes a list of triggers, but can be subclassed to add device-specific logic.
/// </summary>
/// <typeparam name="TTriggerKey">The type of the enum defining all possible trigger keys. 
/// Each of its members must have a description field from which the corresponding trigger's name can be derived.</typeparam>
/// <typeparam name="TTrigger">The type of the <see cref="IncomingTrigger"/> used by this subclass</typeparam>
public abstract class IncomingTriggerDevice<TTriggerKey, TTrigger> : IncomingTriggerDeviceBase<TTriggerKey>
    where TTriggerKey : notnull, Enum
    where TTrigger : IncomingTrigger
{
    /// <summary>
    /// Base constructor.
    /// </summary>
    /// <param name="name">A unique name given to this instance of this subclass of <see cref="IncomingTriggerDevice{TTriggerKey, TTrigger}"/>.</param>
    /// <param name="triggerSettings">An object containing the settings for each <see cref="TTriggerKey"/> within this <see cref="IncomingTriggerDevice{TTriggerKey, TTrigger}"/>.</param>
    /// <param name="serviceState">A <see cref="ServiceState"/> object which will be maintained by the subclass.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="MissingMemberException"></exception>
    public IncomingTriggerDevice(
        string name,
        IncomingTriggerSettings? triggerSettings, 
        ServiceState serviceState
    )
    : base
    (
        name, 
        serviceState
    )
    {
        ImmutableDictionary<TTriggerKey, TTrigger>.Builder dictBuilder = ImmutableDictionary.CreateBuilder<TTriggerKey, TTrigger>();
        if (triggerSettings != null)
        {
            Type t = typeof(TTriggerKey);
            foreach (TTriggerKey value in Enum.GetValues(t))
            {
                string? valueString = value.ToString();
                if (valueString == null)
                {
                    throw new ArgumentException($"{t} cannot contain a enum value the converts to a null string.");
                }
                else
                {
                    object[]? attrs = t.GetField(valueString)?.GetCustomAttributes(typeof(TriggerParameters), false);
                    if (attrs?.FirstOrDefault() is not TriggerParameters attr)
                    {
                        throw new MissingMemberException($"{t} must contain a {nameof(TriggerParameters)} attribute on every member.");
                    }
                    TTrigger trigger = TriggerFactory(triggerSettings.GetOrCreate(value.ToString(), attr.Name, attr.DefaultId, attr.TriggerFilter, attr.DebounceInterval));
                    dictBuilder.Add(value, trigger);
                }
            }
        }
        Triggers = dictBuilder.ToImmutable();
        triggerSettings?.RemoveUntouched();
        TriggersBase = Triggers.ToImmutableDictionary(kvp => kvp.Key, kvp => (IncomingTrigger)kvp.Value);

        UpdateTriggerDict();
        foreach (TTrigger trigger in Triggers.Values)
        {
            trigger.Setting.PropertyChanged += (s, e) => UpdateTriggerDict();
        }
    }

    protected abstract TTrigger TriggerFactory(IncomingTriggerSetting triggerSetting);

    /// <summary>
    /// A dictionary containing a list of all triggers belonging to this object, keyed by <see cref="TTriggerKey"/>, strongly typed as <see cref="TTrigger"/>.
    /// </summary>
    public ImmutableDictionary<TTriggerKey, TTrigger> Triggers { get; }

    /// <summary>
    /// A dictionary containing a list of all triggers belonging to this object, keyed by <see cref="TTriggerKey"/>, widely typed as <see cref="IncomingTrigger"/>.
    /// </summary>
    public override ImmutableDictionary<TTriggerKey, IncomingTrigger> TriggersBase { get; }
    /// <summary>
    /// A dictionary containing a list of all IncomingTrigger objects mapped to each trigger ID.
    /// </summary>
    protected ImmutableDictionary<int, ImmutableList<TTrigger>> _triggerDict;

    private bool _allowDuplicateTriggerIds = false;
    /// <summary>
    /// By default, triggers must have unique IDs. If this property is true, multiple triggers may share an ID,
    /// so that they will all be triggered when a remote trigger is received with that ID.
    /// </summary>
    public bool AllowDuplicateTriggerIds
    {
        get { return _allowDuplicateTriggerIds; }
        set { SetProperty(ref _allowDuplicateTriggerIds, value); }
    }

    private double _progress = 0;
    public double Progress
    {
        get => _progress;
        protected set => _ = SetProperty(ref _progress, value);
    }

    [MemberNotNull(nameof(_triggerDict))]
    private void UpdateTriggerDict()
    {
        //Not using ImmutableDictionary.Builder because we have to transform the list at the end anyway
        Dictionary<int, ImmutableList<TTrigger>.Builder> newTriggerDict = new();

        foreach (TTrigger trigger in Triggers.Values)
        {
            if (newTriggerDict.ContainsKey(trigger.Setting.Id))
            {
                trigger.Setting.IdIsValid = _allowDuplicateTriggerIds;
                if (_allowDuplicateTriggerIds)
                {
                    newTriggerDict[trigger.Setting.Id].Add(trigger);
                }
            }
            else
            {
                ImmutableList<TTrigger>.Builder newList = ImmutableList.CreateBuilder<TTrigger>();
                newList.Add(trigger);
                newTriggerDict.Add(trigger.Setting.Id, newList);
                trigger.Setting.IdIsValid = true;
            }
        }
        _triggerDict = newTriggerDict.ToImmutableDictionary((pair) => pair.Key, (pair) => pair.Value.ToImmutable());
        AfterUpdateTriggerDict();
    }

    protected virtual void AfterUpdateTriggerDict() { }
}
#nullable restore
