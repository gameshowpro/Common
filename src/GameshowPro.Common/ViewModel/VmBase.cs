namespace GameshowPro.Common.ViewModel;

public abstract class VmBase : ObservableClass
{
    private readonly string _dataDir;
    protected VmBase(string dataDir, Func<Task> persistAll, DateTime? buildDate = null, string? versionString = null, ICommand? launchLogCommand = null)
    {
        (string assemblyVersionString, DateTime? assemblyBuildDate) = Assembly.GetCallingAssembly().GetVersionAndBuildDate();
        BuildDate = buildDate ?? assemblyBuildDate ?? DateTime.MinValue;
        VersionString = versionString ?? assemblyVersionString;
        _dataDir = dataDir;
        _ = new DispatcherTimer(TimeSpan.FromSeconds(0.5), DispatcherPriority.SystemIdle, new EventHandler((_, _) =>
        {
            NotifyPropertyChanged(nameof(TimeOfDay));
            DateTime now = DateTime.Now;
            TimeOfDayMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
        }), Dispatcher.CurrentDispatcher);
        _ = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.SystemIdle, new EventHandler((_, _) => NotifyPropertyChanged(nameof(Today))), Dispatcher.CurrentDispatcher);
        ShowDataDirCommand = new RelayCommandSimple(() => ShowDataDir());
        LaunchLogCommand = launchLogCommand;
        PersistAllCommand = new AsyncCommandSimple(persistAll);
    }

    private DateTime _timeOfDayMinute;
    public DateTime TimeOfDayMinute
    {
        get { return _timeOfDayMinute; }
        set { SetProperty(ref _timeOfDayMinute, value); }
    }

    public DateTime TimeOfDay => DateTime.Now;
    public DateTime Today => DateTime.Now;
    public DateTime BuildDate { get; }
    public string VersionString { get; }
    public AsyncCommandSimple PersistAllCommand { get; }
    public RelayCommandSimple ShowDataDirCommand { get; }
    public ICommand? LaunchLogCommand { get; }
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
