namespace GameshowPro.Common.Model;

[method:JsonConstructor]
public class RemoteServiceManagerSettings(ImmutableArray<RemoteServiceGroupSettings>? groups) : ObservableClass
{
    public RemoteServiceManagerSettings() : this(null)
    {
    }

    [DataMember]
    public ImmutableArray<RemoteServiceGroupSettings> Groups
    {
        get;
        set => _ = SetProperty(ref field, value);
    } = groups ?? [];
}

[method: JsonConstructor]
public class RemoteServiceGroupSettings(string? name) : ObservableClass, IIndexed
{
    public RemoteServiceGroupSettings() : this(null)
    {
    }

    public int Index { get; set; }
    [DataMember]
    public string Name
    {
        get => field ?? $"Group {Index + 1}"; //This makes sense because Index is not set until after construction. If not changed before deserialization, this default will be serialized.
        set => _ = SetProperty(ref field, value);
    } = name;
}
