using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using NLog;
using static Barjonas.Common.UtilsWindows;

namespace Barjonas.Common.Wpf
{
    public abstract class AppBase<App, Sys, MainWindow> : Application, IComponentConnector where App : AppBase<App, Sys, MainWindow>, new() where Sys : IDisposable, new() where MainWindow : Window, new()
    {
        private static MainWindow s_mainWindow;
        private static WindowRestoreState s_windowRestoreState;
        private bool _contentLoaded;
        private static Uri s_resourceLocater;
        protected static Logger s_logger = LogManager.GetLogger(typeof(App).ToString());

        static AppBase()
        {
            Utils.SetWpfCulture();
        }

        protected static void BaseMain(string resourceLocater = "app.xaml", DateTime? buildTime = null, bool kioskMode = false)
        {
            s_kioskMode = kioskMode;
            AssemblyName assembly = Assembly.GetEntryAssembly().GetName();
            string process = assembly.Name;
            s_resourceLocater = new Uri($"/{process};component/{resourceLocater}", UriKind.Relative);
            Mutex mutex = new Mutex(false, process);
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    App app = new App();
                    s_logger.Info($"Initializing {process}{(buildTime == null ? "" : $", built {buildTime:s}")}");
                    app.InitializeComponent();
                    app.Run();
                }
                else
                {
                    MessageBox.Show($"Another instance of {process} was already running", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            finally
            {
                mutex?.Close();
            }
        }

        protected Sys _sys;
        protected override void OnStartup(StartupEventArgs e)
        {
            _sys = new Sys();
            s_mainWindow = new MainWindow() { DataContext = _sys };
            s_mainWindow.Show();
            if (s_kioskMode)
            {
                UpdateKioskMode();
            }
            s_mainWindow.Closed += MainWindow_Closed;
        }

        protected virtual void MainWindow_Closed(object sender, EventArgs e)
        {
            s_logger.Info("Main window closed");
            _sys.Dispose();
            s_logger.Info("Sys disposed");
            Current.Shutdown();
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            this._contentLoaded = true;
        }

        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            LoadComponent(this, s_resourceLocater);
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
            if (s_kioskMode)
            {
                s_mainWindow.SetAsKiosk(0, ref s_windowRestoreState);
            }
            else
            {
                s_windowRestoreState?.DoRestore(s_mainWindow);
            }
        }
    }
}
