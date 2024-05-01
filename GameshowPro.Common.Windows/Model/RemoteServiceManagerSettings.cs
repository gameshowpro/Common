namespace GameshowPro.Common.Model;

[method:JsonConstructor]
public class RemoteServiceManagerSettings(ImmutableArray<RemoteServiceGroupSettings>? groups) : NotifyingClass
{
    public RemoteServiceManagerSettings() : this(null)
    {
    }

    private ImmutableArray<RemoteServiceGroupSettings> _groups = groups ?? [];
    [JsonProperty]
    public ImmutableArray<RemoteServiceGroupSettings> Groups
    {
        get => _groups;
        set => _ = SetProperty(ref _groups, value);
    }
}

[method: JsonConstructor]
public class RemoteServiceGroupSettings(string? name) : NotifyingClass, IIndexed
{
    public RemoteServiceGroupSettings() : this(null)
    {
    }

    public int Index { get; set; }
    private string? _name = name;
    [JsonProperty]
    public string Name
    {
        get => _name ?? $"Group {Index + 1}"; //This makes sense because Index is not set until after construction. If not changed before deserialization, this default will be serialized.
        set => _ = SetProperty(ref _name, value);
    }
}
