namespace GameshowPro.Common.Model;

public class IncomingTriggerSetting : NotifyingClass, INotifyDataErrorInfo
{
    protected internal IncomingTriggerSetting() { }

    internal bool _wasTouched;
    public TriggerFilter TriggerFilter { get; set; }

    private string _key = "";
    [JsonProperty, DefaultValue("")]
    public string Key
    {
        get { return _key; }
        set
        {
            if (_key == "")
            {
                _key = value;
            }
            else
            {
                throw new InvalidOperationException("Key can only be set once, never changed.");
            }
        }
    }

    private bool _triggerEdge = true;
    /// <summary>
    /// The direction of the edge which should cause a trigger. 
    /// If true, trigger on rising edge, otherwise trigger on falling edge.
    /// </summary>
    [JsonProperty]
    public bool TriggerEdge
    {
        get => _triggerEdge;
        set => _ = SetProperty(ref _triggerEdge, value);
    }

    private int _id = -1;
    [JsonProperty, DefaultValue(-1)]
    public int Id
    {
        get { return _id; }
        set { SetProperty(ref _id, value); }
    }

    private string _name = "";
    [JsonProperty, DefaultValue("")]
    public string Name
    {
        get { return _name; }
        set { SetProperty(ref _name, value); }
    }

    private bool _isEnabled = true;
    [JsonProperty, DefaultValue(true)]
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set { SetProperty(ref _isEnabled, value); }
    }

    private TimeSpan? _debounceInterval = null;
    [JsonProperty, DefaultValue(null)]
    public TimeSpan? DebounceInterval
    {
        get { return _debounceInterval; }
        set { SetProperty(ref _debounceInterval, value); }
    }

    private bool _idIsValid;
    public bool IdIsValid
    {
        get
        { return _idIsValid; }
        set
        {
            if (SetProperty(ref _idIsValid, value))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Id)));
            }
        }
    }

    public bool HasErrors { get { return !IdIsValid; } }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (!_idIsValid && propertyName == nameof(Id))
        {
            return new List<string>() { "ID is duplicated" };
        }
        else
        {
            return new List<string>();
        }
    }
}
