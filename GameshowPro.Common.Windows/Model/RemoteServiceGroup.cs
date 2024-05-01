namespace GameshowPro.Common.Model;

public class RemoteServiceGroup : IIndexed
{
    internal RemoteServiceGroup(int index, ImmutableArray<IRemoteService> items)
    {
        Index = index;
        Items = items;
    }
    public int Index { get; set; }
    public ImmutableArray<IRemoteService> Items { get; }
    public RemoteServiceGroupSettings? Settings {get; set;}
}
