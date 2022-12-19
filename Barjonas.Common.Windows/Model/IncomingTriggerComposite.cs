namespace Barjonas.Common.Model;

public class IncomingTriggerComposite : IncomingTrigger
{
    public IncomingTriggerComposite(IEnumerable<IncomingTrigger> children, string key, string name)
        : base(new IncomingTriggerSetting()
        {
            Name = name,
            Key = key,
            IsEnabled = true //Default to enabled, could be disabled per-session by user, but not persisted
        }, null)
    {
        Children = children.ToImmutableList();
        foreach (IncomingTrigger child in children)
        {
            child.Triggered += OnVerifiedTrigger;
            child.IsDownChanged += (s, e) => UpdateIsDown();
            child.Setting.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IncomingTriggerSetting.IsEnabled))
                {
                    EnabledChildren = CalculateEnabledChildren;
                }
            };
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
            base.OnVerifiedTrigger(source, args);
        }
    }

    private void UpdateIsDown()
    {
        IsDown = Children.Any(t => t.IsDown == t.Setting.TriggerEdge);
    }

    private ImmutableList<IncomingTrigger> CalculateEnabledChildren 
        => Children.Where(c => c.Setting.IsEnabled).ToImmutableList();

    public ImmutableList<IncomingTrigger> Children { get; }

    private ImmutableList<IncomingTrigger> _enabledChildren;
    public ImmutableList<IncomingTrigger> EnabledChildren
    {
        get { return _enabledChildren; }
        set { SetProperty(ref _enabledChildren, value); }
    }
}
