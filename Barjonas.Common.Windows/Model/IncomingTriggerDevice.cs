// (C) Barjonas LLC 2018

namespace Barjonas.Common.Model;

/// <summary>
/// A base implementation of a trigger device which includes a list of triggers, but can be subclassed to add device-specific logic.
/// </summary>
/// <typeparam name="TTriggerKey">The type of the enum defining all possible trigger keys. 
/// Each of its members must have a description field from which the corresponding trigger's name can be derived.</typeparam>
/// <typeparam name="TTrigger">The type of the <see cref="IncomingTrigger"/> used by this subclass</typeparam>
public abstract class IncomingTriggerDevice<TTriggerKey, TTrigger, TSubclass> : IncomingTriggerDeviceBase<TTriggerKey>
    where TTriggerKey : notnull, Enum
    where TTrigger : IncomingTrigger
    where TSubclass : IIncomingTriggerDeviceBase
{
    private readonly PropertyChangeFilters _propertyChangeFilters = new();
    /// <summary>
    /// Base constructor.
    /// </summary>
    /// <param name="namePrefix">The root of the name given to instance of this subclass of <see cref="IncomingTriggerDevice{TTriggerKey, TTrigger}"/>.</param>
    /// <param name="settings">An object containing the settings for each <see cref="TTriggerKey"/> within this <see cref="IncomingTriggerDevice{TTriggerKey, TTrigger}"/>.</param>
    /// <param name="index">The of index this device to be cross-referenced with default trigger specifications.</param>
    /// <param name="serviceState">A <see cref="ServiceState"/> object which will be maintained by the subclass.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="MissingMemberException"></exception>
    protected IncomingTriggerDevice(
        string namePrefix,
        int index,
        IncomingTriggerDeviceSettingsBase settings,
        ServiceState serviceState
    )
    : base
    (
        namePrefix,
        settings,
        serviceState
    )
    {
        BaseSettings = settings;
        ImmutableDictionary<TTriggerKey, TTrigger>.Builder dictBuilder = ImmutableDictionary.CreateBuilder<TTriggerKey, TTrigger>();
        Type thisType = GetType();
        if (settings.TriggerSettings != null)
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
                    FieldInfo? field = t.GetField(valueString);
                    TriggerParameters? triggerParams = GetTriggerParameters(field);
                    if (triggerParams is null)
                    {
                        throw new MissingMemberException($"{t} must contain a {nameof(TriggerParameters)} attribute on every member.");
                    }
                    ITriggerDefaultSpecification? triggerDefault = GetTriggerDefaultSpecification<TSubclass>(field, index);
                    
                    TTrigger trigger = TriggerFactory(settings.TriggerSettings.GetOrCreate(value.ToString(), triggerParams.Name, triggerDefault?.TriggerId, triggerParams.TriggerFilter, triggerParams.DebounceInterval));
                    dictBuilder.Add(value, trigger);
                }
            }
        }
        Triggers = dictBuilder.ToImmutable();
        settings.TriggerSettings?.RemoveUntouched();
        TriggersBase = Triggers.ToImmutableDictionary(kvp => kvp.Key, kvp => (IncomingTrigger)kvp.Value);

        _propertyChangeFilters.AddFilter((s, e) => UpdateTriggerDict(), Triggers.Values.SelectMany(
            t => 
            new PropertyChangeCondition[] { 
                new (t.Setting, nameof(IncomingTriggerSetting.Id)), 
                new (t.Setting, nameof(IncomingTriggerSetting.IsEnabled)) 
            })
            .Union(new PropertyChangeCondition(BaseSettings, nameof(IncomingTriggerDeviceSettingsBase.AllowDuplicateTriggerIds)))
        );
        _triggerDict ??= ImmutableDictionary<int, ImmutableList<TTrigger>>.Empty;
    }

    /// <summary>
    /// A reference to the device settings object generalized to the base type. The subclass may define a more specific reference.
    /// </summary>
    public IncomingTriggerDeviceSettingsBase BaseSettings { get; }

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
        bool allowDuplicateTriggerIds = BaseSettings.AllowDuplicateTriggerIds;
        foreach (TTrigger trigger in Triggers.Values)
        {
            if (trigger.Setting.IsEnabled)
            {
                if (newTriggerDict.ContainsKey(trigger.Setting.Id))
                {
                    trigger.Setting.IdIsValid = allowDuplicateTriggerIds;
                    if (allowDuplicateTriggerIds)
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
            else
            {
                trigger.Setting.IdIsValid = true;
            }
        }
        _triggerDict = newTriggerDict.ToImmutableDictionary((pair) => pair.Key, (pair) => pair.Value.ToImmutable());
        AfterUpdateTriggerDict();
    }

    protected virtual void AfterUpdateTriggerDict() { }
}
