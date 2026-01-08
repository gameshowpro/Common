// (C) Barjonas LLC 2025

namespace GameshowPro.Common;

public static partial class Utils
{
    /// <summary>
    /// Size a window to fill a given display and configure it to appear without any chrome.
    /// </summary>
    /// <param name="wnd">The Window to size.</param>
    /// <param name="index">The index of the target screen, where zero is always the primary.</param>
    /// <param name="prevState">The previous state of the window, to allow restoring it later.</param>
    public static void SetAsKiosk(this Window wnd, int index, ref WindowRestoreState? prevState)
    {
        WindowRestoreState? restoreTo = prevState;
        prevState = new WindowRestoreState(wnd);
        if (index > -1 && SizeWindowToScreen(wnd, index))
        {
            wnd.Show();
            if (wnd is MahApps.Metro.Controls.MetroWindow mw)
            {
                mw.ShowTitleBar = false;
                mw.ShowCloseButton = false;
            }
            else
            {
                wnd.WindowStyle = WindowStyle.None;
            }
            wnd.ResizeMode = ResizeMode.NoResize;
            if (!Debugger.IsAttached) //Too annoying otherwise
            {
                wnd.Cursor = Cursors.None;
            }
            wnd.Topmost = true;
            wnd.ShowInTaskbar = true;
        }
        else
        {
            wnd.Show();
            restoreTo?.DoRestore(wnd);
        }
    }

    public class WindowRestoreState
    {
        private Rectangle _rect;
        private readonly WindowStyle _style;
        private readonly bool _useNone;
        private readonly ResizeMode _mode;
        private readonly System.Windows.Input.Cursor _cursor;
        private readonly bool _topMost;
        private readonly bool _taskBar;
        internal WindowRestoreState(Window wnd)
        {
            _rect = new Rectangle((int)wnd.Left, (int)wnd.Top, (int)wnd.Width, (int)wnd.Height);
            _mode = wnd.ResizeMode;
            _cursor = wnd.Cursor;
            _topMost = wnd.Topmost;
            _taskBar = wnd.ShowInTaskbar;
            if (wnd is MahApps.Metro.Controls.MetroWindow mw)
            {
                _useNone = mw.ShowTitleBar;
            }
            else
            {
                _style = wnd.WindowStyle;
            }
        }

        internal void DoRestore(Window wnd)
        {
            wnd.SizeWindowToRect(_rect);
            wnd.ResizeMode = _mode;
            wnd.Cursor = _cursor;
            wnd.Topmost = _topMost;
            wnd.ShowInTaskbar = _taskBar;
            if (wnd is MahApps.Metro.Controls.MetroWindow mw)
            {
                mw.ShowTitleBar = _useNone;
            }
            else
            {
                wnd.WindowStyle = _style;
            }
        }
    }

    /// <summary>
    /// Size a window to fill a given display.
    /// </summary>
    /// <param name="window">The Window to size.</param>
    /// <param name="index">The index of the target screen, where zero is always the primary.</param>
    public static bool SizeWindowToScreen(this Window window, int index)
    {
        Rectangle? target = null;
        if (index == 0)
        {
            target = System.Windows.Forms.Screen.PrimaryScreen?.Bounds;
        }
        else
        {
            int i = 0;
            foreach (System.Windows.Forms.Screen d in System.Windows.Forms.Screen.AllScreens)
            {
                if (!d.Primary)
                {
                    i++;
                    if (i == index)
                    {
                        target = d.Bounds;
                        break;
                    }
                }
            }
        }
        if (target.HasValue && target.Value.Width > 0 && target.Value.Height > 0)
        {
            SizeWindowToRect(window, target.Value);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void SizeWindowToRect(this Window window, Rectangle target)
    {
        window.Left = target.Left;
        window.Top = target.Top;
        window.Width = target.Width;
        window.Height = target.Height;
    }

    /// <summary>
    /// Returns true if process is currently running with elevated permissions.
    /// </summary>
    public static bool IsElevated
    {
        get
        {
            return WindowsIdentity.GetCurrent().Owner?.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) == true;
        }
    }

    public static bool IsAdministrator()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();

        if (identity != null)
        {
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        return false;
    }

    /// <summary>
    /// Try to convert a given string to a Windows Media Color, based on ColorConverter but with potential added functionality in future.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="color"></param>
    public static bool TryStringToColor(string input, [NotNullWhen(true)] out System.Windows.Media.Color? color)
    {
        try
        {
            if (System.Windows.Media.ColorConverter.ConvertFromString(input) is System.Windows.Media.Color c)
            {
                color = c;
                return true;
            }
        }
        catch (Exception)
        { }
        color = null;
        return false;
    }

    public static string ToRgbHexString(this System.Windows.Media.Color color)
        => $"#{color.R:x2}{color.G:x2}{color.B:x2}";


    /// <summary>
    /// Create a <see cref="System.Windows.Media.Color"/> from an array of 4 bytes representing RGBW. The result will be visually similar to an RGBW LED set to the given values.
    /// </summary>
    public static System.Windows.Media.Color ColorFromRGBW(int[] rgbw)
    {
        if (rgbw.Length != 4)
        {
            throw new ArgumentException("The byte array must contain exactly 4 elements.", nameof(rgbw));
        }
        byte newR = (byte)Math.Min(255, rgbw[0] + rgbw[3]);
        byte newG = (byte)Math.Min(255, rgbw[1] + rgbw[3]);
        byte newB = (byte)Math.Min(255, rgbw[2] + rgbw[3]);

        return System.Windows.Media.Color.FromRgb(newR, newG, newB);
    }

    public static string? FileFromDialog(Uri basePath, string? startPath, string filters, string prompt, bool save)
    {
        string absStartPath = startPath.MakePathAbsolute(basePath);
        string? absStartDir = Path.GetDirectoryName(absStartPath);
        string initDir, defaultFilename;
        if (startPath != null && Directory.Exists(absStartDir))
        {
            initDir = absStartDir;
            defaultFilename = Path.GetFileName(absStartPath);
        }
        else
        {
            initDir = basePath.LocalPath;
            defaultFilename = "";
        }
        using CommonFileDialog dlg = save ?
            new CommonSaveFileDialog()
            {
                Title = prompt,
                AddToMostRecentlyUsedList = true,
                ShowPlacesList = true,
                DefaultFileName = defaultFilename,
                InitialDirectory = initDir,

            }
        : 
            new CommonOpenFileDialog()
            {
                Title = prompt,
                IsFolderPicker = false,
                AddToMostRecentlyUsedList = true,
                ShowPlacesList = true,
                DefaultFileName = defaultFilename,
                InitialDirectory = initDir,

            };
        {
            string[] filter = filters.Split('|');
            dlg.Filters.Add(new CommonFileDialogFilter(filter[0], filter[1]));
            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                return dlg.FileName;
            }
        }
        return startPath;
    }

    public static void WaitForServices(ImmutableList<string> services, TimeSpan serviceRunTimeout, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (services.IsEmpty)
        {
            logger.LogInformation("Configuration does not contain any services to wait for, so no need to wait");
        }
        foreach (string service in services)
        {
            ServiceController? sc = null;
            ServiceControllerStatus? status = null;
            try
            {
                sc = new ServiceController(service);
                status = sc.Status;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while trying to find service {service}, so can't wait on it", service);
            }
            if (status != null && sc != null)
            {
                if (status.Value == ServiceControllerStatus.Running)
                {
                    logger.LogInformation("Service {service} is running, so no need to wait", service);
                }
                else
                {
                    logger.LogInformation("Service {service} is {state}, so waiting for it now", service, status.Value);
                    try
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Running, serviceRunTimeout);
                        logger.LogInformation("Continuing now that service {service} is running", service);
                    }
                    catch (System.ServiceProcess.TimeoutException)
                    {
                        logger.LogError("Continuing after giving up waiting for {service} to start running", service);
                    }
                }
            }
        }
    }

    public static IDispatcher WindowsDispatcher { get; } = new DefaultWindowsDispatcher();

    private class DefaultWindowsDispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        bool IDispatcher.CheckAccess()
            => _dispatcher.CheckAccess();

        void IDispatcher.BeginInvoke(Delegate method, params object[] args)
            => _ = _dispatcher.BeginInvoke(method, DispatcherPriority.DataBind, args);
    }
}
