// (C) Barjonas LLC 2019
using GameshowPro.Common.View;

namespace GameshowPro.Common;

public static partial class UtilsWindows
{
    /// <summary>
    /// Size a window to fill a given display and configure it to appear without any chrome.
    /// </summary>
    /// <param name="wnd">The Window to size.</param>
    /// <param name="index">The index of the target screen, where zero is always the primary.</param>
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
        WindowsIdentity identity = WindowsIdentity.GetCurrent();

        if (identity != null)
        {
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        return false;
    }

    public static FrozenDictionary<TTriggerKey, IncomingTriggerComposite> ComposeDevicesIntoTriggerDictionary<TTriggerKey>(
        IEnumerable<IncomingTriggerDeviceBase<TTriggerKey>> devices,
        ILoggerFactory loggerFactory)
            where TTriggerKey : struct, Enum
        => Enum.GetValues<TTriggerKey>().ToFrozenDictionary(
                t => t, 
                t => new IncomingTriggerComposite(
                    devices.Select(d => d.TriggersBase[t]), 
                    t.ToString(), 
                    GetTriggerParameters(t.GetType().GetField(t.ToString()))?.Name ?? "No name",
                    loggerFactory.CreateLogger($"{nameof(IncomingTriggerComposite)}({t})")
                )
            );

    internal static TriggerParameters? GetTriggerParameters(MemberInfo? enumMemberInfo)
    {
        object[]? attrs = enumMemberInfo?.GetCustomAttributes(typeof(TriggerParameters), false);
        if (attrs?.FirstOrDefault() is not TriggerParameters attr)
        {
            return null;
        }
        return attr;
    }

    internal static ITriggerDefaultSpecification? GetTriggerDefaultSpecification<TTriggerDevice>(MemberInfo? enumMemberInfo, int deviceInstanceIndex)
        where TTriggerDevice : IIncomingTriggerDeviceBase
    {
        return enumMemberInfo?.GetCustomAttributes(typeof(TriggerDefaultSpecification<TTriggerDevice>))?
            .Select(a => a as TriggerDefaultSpecification<TTriggerDevice>)
            .Where(s =>
                s is not null &&
                s.ParentDeviceInstanceIndex == deviceInstanceIndex
            )
            .FirstOrDefault();
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


    public static string? FileFromDialog(Uri basePath, string? startPath, string filters, string prompt)
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
        using (var dlg = new CommonOpenFileDialog()
        {
            Title = prompt,
            IsFolderPicker = false,
            AddToMostRecentlyUsedList = true,
            ShowPlacesList = true,
            DefaultFileName = defaultFilename,
            InitialDirectory = initDir,

        })
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
}
