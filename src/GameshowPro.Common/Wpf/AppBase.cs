using System.Windows.Markup;

namespace GameshowPro.Common.Wpf;

public abstract class AppBase<App, Sys, MainWindow>(ILoggerFactory loggerFactory) : Application, IComponentConnector where App : AppBase<App, Sys, MainWindow> where MainWindow : Window, new()
{
    private static Func<IEnumerable<Window>>? s_windowsFactory;
    private static IEnumerable<Window>? s_windows;
    private static WindowRestoreState? s_windowRestoreState;
    private bool _contentLoaded;
    private static Uri? s_resourceLocator;
    protected readonly ILoggerFactory _loggerFactory = loggerFactory;
    protected readonly ILogger _logger = loggerFactory.CreateLogger("Application");
    protected readonly CancellationTokenSource _cancellationTokenSource = new();

    static AppBase()
    {
        Utils.SetWpfCulture();
    }

    protected static void BaseMain(Func<ILoggerFactory, App> appFactory, ILoggerFactory loggerFactory, string resourceLocater = "app.xaml", DateTime? buildTime = null, bool kioskMode = false, Func<IEnumerable<Window>>? windowsFactory = null)
    {
        s_windowsFactory = windowsFactory ?? (new(() => [new MainWindow()]));
        s_kioskMode = kioskMode;
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        AssemblyName? assemblyName = entryAssembly?.GetName();
        buildTime ??= entryAssembly?.GetBuildDate();
        string? process = assemblyName?.Name;
        Version? version = assemblyName?.Version;

        if (IsAdministrator())
        {
            _ = MessageBox.Show($"{process} can not be run as administrator.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }
        if (process != null)
        {
            s_resourceLocator = new Uri($"/{process};component/{resourceLocater}", UriKind.Relative);
            Mutex mutex = new(false, process);
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    ILogger logger = loggerFactory.CreateLogger("AppBaseMain");
                    App app = appFactory(loggerFactory);

                    logger.LogInformation("Initializing {process} v{version} built {buildTime)}", process, version, buildTime);
                    app.InitializeComponent();
                    _ = app.Run();
                }
                else
                {
                    _ = MessageBox.Show($"Another instance of {process} was already running", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            finally
            {
                mutex?.Close();
            }
        }
    }

    abstract protected Sys CreateSys(ILoggerFactory loggerFactory, CancellationToken cancellationToken);

    protected Sys? _sys;
    protected override void OnStartup(StartupEventArgs e)
    {
        _sys = CreateSys(_loggerFactory, _cancellationTokenSource.Token);
        _logger.LogTrace("Creating windows");
        s_windows = s_windowsFactory?.Invoke();
        int index = 0;
        foreach (Window window in s_windows.NeverNull())
        {
            window.DataContext = _sys;
            window.Show();
            _logger.LogTrace("Showing window {index} of type {window}", index, window.GetType().Name);
            if (index == 0)
            {
                window.Closed += MainWindow_Closed;
            }
            index++;
        }
        if (s_kioskMode)
        {
            UpdateKioskMode();
        }
    }

    protected virtual async void MainWindow_Closed(object? sender, EventArgs e)
    {
        _logger.LogInformation("Main window closed");
        _cancellationTokenSource.Cancel();
        _logger.LogInformation("App cancellation token cancelled");
        if (_sys != null)
        {
            if (_sys is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            if (_sys is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _logger.LogInformation("Sys disposed");
        Current.Shutdown();
    }

    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target) => this._contentLoaded = true;

    public void InitializeComponent()
    {
        if (_contentLoaded)
        {
            return;
        }
        _contentLoaded = true;
        LoadComponent(this, s_resourceLocator);
    }

    private static bool s_kioskMode = false;
    public static bool KioskMode
    {
        get => s_kioskMode;
        set
        {
            if (value != s_kioskMode)
            {
                s_kioskMode = value;
                UpdateKioskMode();
            }
        }
    }

    private static void UpdateKioskMode()
    {
        int index = 0;
        foreach (Window window in s_windows.NeverNull())
        {
            UpdateKioskMode(window, index);
            index++;
        }
    }

    private static void UpdateKioskMode(Window window, bool kioskMode, int index)
    {
        if (kioskMode)
        {
            window.SetAsKiosk(index, ref s_windowRestoreState);
        }
        else
        {
            s_windowRestoreState?.DoRestore(window);
        }
    }

    private static void UpdateKioskMode(Window window, int index)
        => UpdateKioskMode(window, s_kioskMode, index);

    protected void UpdateKioskMode(int windowIndex, bool kioskMode, int index)
    {
        Window? window = s_windows?.ElementAtOrDefault(windowIndex);
        if (window != null)
        {
            UpdateKioskMode(window, kioskMode, index);
        }
    }
}
