using System;
using System.Collections.Generic;
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
        private static Func<IEnumerable<Window>> s_windowsFactory;
        private static IEnumerable<Window> s_windows;
        private static WindowRestoreState s_windowRestoreState;
        private bool _contentLoaded;
        private static Uri s_resourceLocater;
        protected static readonly Logger s_logger = LogManager.GetLogger(typeof(App).ToString());

        static AppBase()
        {
            Utils.SetWpfCulture();
        }

        protected static void BaseMain(string resourceLocater = "app.xaml", DateTime? buildTime = null, bool kioskMode = false, Func<IEnumerable<Window>> windowsFactory = null)
        {
            if (windowsFactory == null)
            {
                s_windowsFactory = new(() => new List<Window> { new MainWindow() });
            }
            else
            {
                s_windowsFactory = windowsFactory;
            }
            s_kioskMode = kioskMode;
            AssemblyName assembly = Assembly.GetEntryAssembly().GetName();
            string process = assembly.Name;
            s_resourceLocater = new Uri($"/{process};component/{resourceLocater}", UriKind.Relative);
            Mutex mutex = new(false, process);
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    App app = new();
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
            s_windows = s_windowsFactory.Invoke();
            int index = 0;
            foreach (Window window in s_windows)
            {
                window.DataContext = _sys;
                window.Show();
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
            int index = 0;
            foreach (Window window in s_windows)
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
}
