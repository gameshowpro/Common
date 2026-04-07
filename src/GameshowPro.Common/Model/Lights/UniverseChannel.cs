// (C) Barjonas LLC 2018

namespace GameshowPro.Common.Model.Lights;

public class UniverseChannel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly Universe _universe;

    /// <summary>
    /// Channel constructor.
    /// </summary>
    /// <param name="universe">The universe to which this channel belongs.</param>
    /// <param name="index">The zero-based index of this channel.</param>
    internal UniverseChannel(Universe universe, int index)
    {
        Index = index;
        _universe = universe;
    }

    /// <summary>
    /// Zero-based channel index.
    /// </summary>
    public int Index { get; }

    public byte Level
    {
        get { return _universe._data[Index + 1]; }
        set
        {
            //Note that all notifications are triggered by the parent universe.  This is just a view model.
            _universe.SetData(Index, value);
        }
    }

    internal void LevelChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
    }

    internal void MasterChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MasterChannel)));
    }

    /// <summary>
    /// This is a abstract lighting channel bound into a client object model.  If set, the Dmx.Channel class will act as a slave to the master's values.
    /// </summary>
    public FixtureChannel? MasterChannel { get; set
        {
            if (field != value)
            {
                field?.LevelChanged -= MasterChannel_LevelChanged;
                field = value;
                Level = field?.Level ?? 0;
                field?.LevelChanged += MasterChannel_LevelChanged;
                MasterChanged();
            }
        } }

    private void MasterChannel_LevelChanged(object? sender, byte e)
    {
        Level = e;
    }
}
