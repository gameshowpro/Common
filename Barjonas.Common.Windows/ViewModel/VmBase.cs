namespace Barjonas.Common.ViewModel;

public abstract class VmBase : NotifyingClass
{
    protected static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private readonly string _dataDir;
    public VmBase(string dataDir, Action persistAll, DateTime? buildDate = null, string? versionString = null)
    {
        BuildDate = buildDate ?? DateTime.MinValue;
        VersionString = versionString ?? Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "Unknown";
        _dataDir = dataDir;
        _todUpdater = new DispatcherTimer(TimeSpan.FromSeconds(0.5), DispatcherPriority.SystemIdle, new EventHandler((o, e) =>
        {
            NotifyPropertyChanged(nameof(TimeOfDay));
            DateTime now = DateTime.Now;
            TimeOfDayMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
        }), Dispatcher.CurrentDispatcher);
        _ = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.SystemIdle, new EventHandler((o, e) => NotifyPropertyChanged(nameof(Today))), Dispatcher.CurrentDispatcher);
        ShowDataDirCommand = new RelayCommandSimple(() => ShowDataDir());
        LaunchNLogLogCommand = new RelayCommand<string?>((string? targetKey) => LaunchCurrentNLogLog(targetKey ?? "f"));
        PersistAllCommand = new RelayCommandSimple(persistAll);
    }

    private DateTime _timeOfDayMinute;
    public DateTime TimeOfDayMinute
    {
        get { return _timeOfDayMinute; }
        set { SetProperty(ref _timeOfDayMinute, value); }
    }

    private readonly DispatcherTimer _todUpdater;
    public DateTime TimeOfDay => DateTime.Now;
    public DateTime Today => DateTime.Now;
    public DateTime BuildDate { get; }
    public string VersionString { get; }
    public RelayCommandSimple PersistAllCommand { get; private set; }
    public RelayCommandSimple ShowDataDirCommand { get; private set; }
    public RelayCommand<string?> LaunchNLogLogCommand { get; private set; }
    protected virtual void ShowDataDir()
    {
        ProcessStartInfo info = new()
        {
            FileName = "explorer.exe",
            Arguments = _dataDir,
            UseShellExecute = true
        };
        try
        {
            Process.Start(info);
        }
        catch
        {
        }
    }
}
