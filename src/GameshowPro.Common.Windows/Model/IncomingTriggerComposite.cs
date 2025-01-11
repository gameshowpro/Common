namespace GameshowPro.Common.Model;

public class IncomingTriggerComposite : IncomingTrigger
{
    public IncomingTriggerComposite(IEnumerable<IncomingTrigger> children, string key, string name, ILogger logger)
        : base(new IncomingTriggerSetting()
        {
            Name = name,
            Key = key,
            IsEnabled = true //Default to enabled, could be disabled per-session by user, but not persisted

        }, null, logger)
    {
        Children = [.. children];
        foreach (IncomingTrigger child in children)
        {
            child.Triggered += OnVerifiedTrigger;
            child.IsDownChanged += (s, e) => UpdateIsDown();
            child.Setting.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(IncomingTriggerSetting.TriggerEdge):
                        UpdateIsDown();
                        break;
                    case nameof(IncomingTriggerSetting.IsEnabled):
                        EnabledChildren = CalculateEnabledChildren;
                        UpdateIsDown();
                        break;
                }
            };
            if (child.ParentDevice != null)
            {
                child.ParentDevice.BaseSettings.PropertyChanged += (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(IncomingTriggerDeviceSettingsBase.IsEnabled):
                            EnabledChildren = CalculateEnabledChildren;
                            UpdateIsDown();
                            break;
                    }
                };
            }
            child.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Ordinal):
                    case nameof(Time):
                        IncomingTrigger? c = s as IncomingTrigger;
                        if (c is not null)
                        {
                            Ordinal = c.Ordinal;
                            Time = c.Time;
                        }
                        break;
                }
            };
        }
        _enabledChildren = CalculateEnabledChildren;
        UpdateIsDown();
    }

    public override bool BaseClassDetectsEdges => false; //Don't allow the base class to call OnConfiguredEdge()

    protected override void OnVerifiedTrigger(object? source, TriggerArgs args)
    {
        if (Setting.IsEnabled)
        {
            if (source is IncomingTrigger trigger)
            {
                if (TriggerIsEnabled(trigger))
                {
                    //The PropertyChanged event might come later. IncomingTrigger subclasses should have updated these properties before calling here.
                    Ordinal = trigger.Ordinal;
                    Time = trigger.Time;
                    IsTest = trigger.IsTest;
                    _logger.LogTrace("Received trigger from {source}, ordinal {ordinal}, time {time}{IsTest}", source.GetType(), Ordinal, Time, IsTest ? " TEST ONLY" : "");
                    base.OnVerifiedTrigger(source, args);
                }
            }
        }
    }

    private void UpdateIsDown()
    {
        IsDown = _enabledChildren.Any(t => t.IsDown == t.Setting.TriggerEdge);
    }

    private ImmutableList<IncomingTrigger> CalculateEnabledChildren 
        => [.. Children.Where(TriggerIsEnabled)];

    private static bool TriggerIsEnabled(IncomingTrigger trigger)
        => trigger.Setting.IsEnabled && trigger.ParentDevice?.BaseSettings.IsEnabled != false;

    public ImmutableList<IncomingTrigger> Children { get; }

    private ImmutableList<IncomingTrigger> _enabledChildren;
    public ImmutableList<IncomingTrigger> EnabledChildren
    {
        get { return _enabledChildren; }
        set { SetProperty(ref _enabledChildren, value); }
    }
}
