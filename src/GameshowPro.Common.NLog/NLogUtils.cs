using System.IO;

namespace GameshowPro.Common;

public static class NLogUtils
{
    /// <summary>
    /// Return the path of the NLog log file which is currently receiving events from the specified target.
    /// </summary>
    /// <param name="targetName">The name of the target, as defined in the NLog config, e.g. "f"</param>
    /// <returns></returns>
    public static string? CurrentNLogLogPath(string targetName)
    {
        NLog.Targets.Target target = LogManager.Configuration.FindTargetByName(targetName);
        switch (target)
        {
            case NLog.Targets.FileTarget ft:
                return FileTargetToPath(ft);
            case NLog.Targets.Wrappers.AsyncTargetWrapper atw:
                if (atw.WrappedTarget is NLog.Targets.FileTarget wft)
                {
                    return FileTargetToPath(wft);
                }
                break;
        }
        return null;
    }

    private static string FileTargetToPath(NLog.Targets.FileTarget ft)
    {
        return Path.GetFullPath(ft.FileName.Render(new LogEventInfo { TimeStamp = DateTime.Now }));
    }

    /// <summary>
    /// Launch VS Code with the current file specified by the given NLog target.
    /// </summary>
    /// <param name="targetName">The name of the target, as defined in the NLog config, e.g. "f"</param>
    /// <returns></returns>
    public static bool LaunchCurrentNLogLog(string targetName)
    {
        string? path = CurrentNLogLogPath(targetName);
        if (!File.Exists(path))
        {
            return false;
        }
        UriBuilder uri = new("vscode", "file") { Path = path + ":999999:0" };

        ProcessStartInfo info = new()
        {
            FileName = uri.Uri.AbsoluteUri,
            //Arguments = "\"" + path + "\"",
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true,
            Verb = "open"
        };
        try
        {
            return Process.Start(info) != null;
        }
        catch
        {
            return false;
        }
    }

    public static ICommand GetLaunchNLogLogCommand(string defaultTargetName)
        => new RelayCommand<string?>((string? targetKey) => LaunchCurrentNLogLog(targetKey ?? defaultTargetName));
}
