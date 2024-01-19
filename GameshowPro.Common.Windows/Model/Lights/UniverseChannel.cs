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
        _index = index;
        _universe = universe;
    }

    private readonly int _index;
    /// <summary>
    /// Zero-based channel index.
    /// </summary>
    public int Index
    {
        get { return _index; }
    }

    public byte Level
    {
        get { return _universe._data[_index + 1]; }
        set
        {
            //Note that all notifications are triggered by the parent universe.  This is just a view model.
            _universe.SetData(_index, value);
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

    private FixtureChannel? _masterChannel;
    /// <summary>
    /// This is a abstract lighting channel bound into a client object model.  If set, the Dmx.Channel class will act as a slave to the master's values.
    /// </summary>
    public FixtureChannel? MasterChannel
    {
        get { return _masterChannel; }
        set
        {
            if (_masterChannel != value)
            {
                if (_masterChannel != null)
                {
                    _masterChannel.LevelChanged -= MasterChannel_LevelChanged;
                }
                _masterChannel = value;
                Level = _masterChannel?.Level ?? 0;
                if (_masterChannel != null)
                {
                    _masterChannel.LevelChanged += MasterChannel_LevelChanged;
                }
                MasterChanged();
            }
        }
    }

    private void MasterChannel_LevelChanged(object? sender, byte e)
    {
        Level = e;
    }
}
