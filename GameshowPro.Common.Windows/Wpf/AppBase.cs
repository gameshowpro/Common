using System.Windows.Markup;

namespace GameshowPro.Common.Wpf;

public abstract class AppBase<App, Sys, MainWindow> : Application, IComponentConnector where App : AppBase<App, Sys, MainWindow>, new() where Sys : IDisposable, new() where MainWindow : Window, new()
{
    private static Func<IEnumerable<Window>>? s_windowsFactory;
    private static IEnumerable<Window>? s_windows;
    private static WindowRestoreState? s_windowRestoreState;
    private bool _contentLoaded;
    private static Uri? s_resourceLocator;
    protected static readonly Logger s_logger = LogManager.GetLogger(typeof(App).ToString());

    static AppBase()
    {
        Utils.SetWpfCulture();
    }

    protected static void BaseMain(string resourceLocater = "app.xaml", DateTime? buildTime = null, bool kioskMode = false, Func<IEnumerable<Window>>? windowsFactory = null)
    {
        s_windowsFactory = windowsFactory ?? (new(() => [new MainWindow()]));
        s_kioskMode = kioskMode;
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        AssemblyName? assemblyName = entryAssembly?.GetName();
        buildTime ??= entryAssembly?.GetBuildDate();
        string? process = assemblyName?.Name;
        Version? version = assemblyName?.Version;
        if (process != null)
        {
            s_resourceLocator = new Uri($"/{process};component/{resourceLocater}", UriKind.Relative);
            Mutex mutex = new(false, process);
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    App app = new();
                    s_logger.Info($"Initializing {process}{(version == null ? "" : $"v{version}")}{(buildTime == null ? "" : $", built {buildTime:s}")}");
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

    protected Sys? _sys;
    protected override void OnStartup(StartupEventArgs e)
    {
        _sys = new Sys();
        s_logger.Trace("Creating windows");
        s_windows = s_windowsFactory?.Invoke();
        int index = 0;
        foreach (Window window in s_windows.NeverNull())
        {
            window.DataContext = _sys;
            window.Show();
            s_logger.Trace("Showing window {index} of type {window}", index, window.GetType().Name);
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

    protected virtual void MainWindow_Closed(object? sender, EventArgs e)
    {
        s_logger.Info("Main window closed");
        _sys?.Dispose();
        s_logger.Info("Sys disposed");
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

    private static void UpdateKioskMode(Window window, int index)
    {
        if (s_kioskMode)
        {
            window.SetAsKiosk(index, ref s_windowRestoreState);
        }
        else
        {
            s_windowRestoreState?.DoRestore(window);
        }
    }
}
