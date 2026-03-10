namespace GameshowPro.Common.Model;

public class IncomingTriggerSetting : ObservableClass, INotifyDataErrorInfo
{
    protected internal IncomingTriggerSetting() { }

    internal bool _wasTouched;
    public TriggerFilter TriggerFilter { get; set; }

    [DataMember, DefaultValue("")]
    public string Key
    {
        get;
        set
        {
            if (field == "")
            {
                field = value;
            }
            else
            {
                throw new InvalidOperationException("Key can only be set once, never changed.");
            }
        }
    } = "";

    /// <summary>
    /// The direction of the edge which should cause a trigger.
    /// If true, trigger on rising edge, otherwise trigger on falling edge.
    /// </summary>
    [DataMember]
    public bool TriggerEdge
    {
        get;
        set => _ = SetProperty(ref field, value);
    } = true;

    [DataMember, DefaultValue(-1)]
    public int Id
    {
        get;
        set { SetProperty(ref field, value); }
    } = -1;

    [DataMember, DefaultValue("")]
    public string Name
    {
        get;
        set { SetProperty(ref field, value); }
    } = "";

    [DataMember, DefaultValue(true)]
    public bool IsEnabled
    {
        get;
        set { SetProperty(ref field, value); }
    } = true;

    [DataMember, DefaultValue(null)]
    public TimeSpan? DebounceInterval
    {
        get;
        set { SetProperty(ref field, value); }
    } = null;

    public bool IdIsValid
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Id)));
            }
        }
    }

    public bool HasErrors { get { return !IdIsValid; } }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (!IdIsValid && propertyName == nameof(Id))
        {
            return new List<string>() { "ID is duplicated" };
        }
        else
        {
            return new List<string>();
        }
    }
}
