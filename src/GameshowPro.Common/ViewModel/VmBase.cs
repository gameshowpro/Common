namespace GameshowPro.Common.ViewModel;

public abstract class VmBase : ObservableClass
{
    private readonly string _dataDir;
    /// <summary>
    /// Modern overload - build date and version string are automatically obtained from the calling assembly, and build date is converted to local time
    /// </summary>
    protected VmBase(string dataDir, Func<Task> persistAll, DateTimeOffset? buildDate = null, ICommand? launchLogCommand = null)
    : this(dataDir, persistAll, buildDate?.LocalDateTime, null, launchLogCommand)
    { }

    /// <summary>
    /// Legacy overload
    /// </summary>
    protected VmBase(string dataDir, Func<Task> persistAll, DateTime? buildDate = null, string? versionString = null, ICommand? launchLogCommand = null)
    {
        (string assemblyVersionString, DateTime? assemblyBuildDate) = Assembly.GetEntryAssembly()?.GetVersionAndBuildDate() ?? (string.Empty, null);
        BuildDate = buildDate ?? assemblyBuildDate ?? DateTime.MinValue;
        VersionString = versionString ?? assemblyVersionString;
        _dataDir = dataDir;
        _ = new DispatcherTimer(TimeSpan.FromSeconds(0.5), DispatcherPriority.SystemIdle, new EventHandler(OnTimeOfDayTick), Dispatcher.CurrentDispatcher);
        _ = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.SystemIdle, new EventHandler((_, _) => NotifyPropertyChanged(nameof(Today))), Dispatcher.CurrentDispatcher);
        ShowDataDirCommand = new RelayCommandSimple(() => ShowDataDir());
        LaunchLogCommand = launchLogCommand;
        PersistAllCommand = new AsyncCommandSimple(persistAll);

        void OnTimeOfDayTick(object? sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(TimeOfDay));
            DateTime now = DateTime.Now;
            TimeOfDayMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
        }
    }

    public DateTime TimeOfDayMinute
    {
        get;
        set { SetProperty(ref field, value); }
    }

#pragma warning disable CA1822 // Must stay instance members: XAML binds to them and the timers above raise PropertyChanged for them; making them static would change the public API.
    public DateTime TimeOfDay => DateTime.Now;
    public DateTime Today => DateTime.Now;
#pragma warning restore CA1822
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
            _ = Process.Start(info);
        }
        catch
        {
        }
    }
}
