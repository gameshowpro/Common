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
        // Ensure configuration and target are present to avoid nullability issues
        var config = LogManager.Configuration;
        if (config is null)
        {
            return null;
        }
        NLog.Targets.Target? target = config.FindTargetByName(targetName);
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
        string rendered = ft.FileName?.Render(new LogEventInfo { TimeStamp = DateTime.Now }) ?? string.Empty;
        return Path.GetFullPath(rendered);
    }

    /// <summary>
    /// Launch VS Code opening the current log file for a given NLog target.
    /// <remarks>Docs added by AI.</remarks>
    /// </summary>
    /// <param name="targetName">The name of the NLog target (e.g., "f").</param>
    /// <returns>True if the process was started, otherwise false.</returns>
    public static bool LaunchCurrentNLogLog(string targetName)
    {
        string? path = CurrentNLogLogPath(targetName);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
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

    /// <summary>
    /// Create a command that launches the current log file for an NLog target, defaulting when none provided.
    /// <remarks>Docs added by AI.</remarks>
    /// </summary>
    /// <param name="defaultTargetName">Fallback target name if the command parameter is null.</param>
    public static ICommand GetLaunchNLogLogCommand(string defaultTargetName)
        => new RelayCommand<string?>((string? targetKey) => LaunchCurrentNLogLog(targetKey ?? defaultTargetName));
}
