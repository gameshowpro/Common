namespace GameshowPro.Common.Model;

/// <summary>What a <see cref="IClockDisciplineProbe"/> found out about the wall clock.</summary>
/// <param name="Quality">How well the wall clock is disciplined. <see cref="ClockQuality.System"/> whenever the operating system cannot say, or says "not synchronized".</param>
/// <param name="EstimatedError">The time service's own estimate of the maximum error against its source; null = unknown. Not a measurement.</param>
/// <param name="AbsoluteClockId">Names the source (for example <c>ntp:time.example.com</c>) or, when there is none, this machine's free-running clock (<c>system:{machine id}</c>).</param>
/// <param name="Detail">Why the quality is what it is, for logs.</param>
/// <param name="Failed">True if the probe could not get an answer (error, timeout, access denied, unrecognized output). Quality is then <see cref="ClockQuality.System"/>.</param>
public sealed record ClockDisciplineReading(ClockQuality Quality, TimeSpan? EstimatedError, string AbsoluteClockId, string Detail, bool Failed = false)
{
    /// <summary>This machine's own free-running clock: the identity to use when no source can be named.</summary>
    public static string FreeRunningId => $"system:{MonotonicClock.MachineId}";

    /// <summary>A reading of a clock that is not (known to be) disciplined.</summary>
    public static ClockDisciplineReading FreeRunning(string detail) => new(ClockQuality.System, null, FreeRunningId, detail);

    /// <summary>A reading for a probe that could not get an answer.</summary>
    public static ClockDisciplineReading Unknown(string reason) => new(ClockQuality.System, null, FreeRunningId, reason, true);
}

/// <summary>
/// Finds out whether, and how well, the machine's wall clock is disciplined. Implementations must never throw and must be bounded in time;
/// they may be slow (a child process) and so are only called from a slow timer, never on the timestamp-conversion path.
/// </summary>
public interface IClockDisciplineProbe
{
    ClockDisciplineReading Probe();
}

/// <summary>
/// The default probe for the current platform: the Windows Time service on Windows (<see cref="WindowsTimeServiceProbe"/>), the kernel's
/// NTP state via adjtimex on 64-bit Linux (<see cref="LinuxAdjtimexProbe"/>), and "free-running, unsupported platform" elsewhere.
/// Third-party agents (Domain Time II, Dante's PTP client, ptp4l with phc2sys and so on) are not detected: such a machine reads as
/// <see cref="ClockQuality.System"/> with unknown error, unless the OS itself reports it synchronized.
/// </summary>
public sealed class SystemClockDisciplineProbe : IClockDisciplineProbe
{
    public static readonly SystemClockDisciplineProbe Instance = new();

    private readonly IClockDisciplineProbe _inner =
        OperatingSystem.IsWindows() ? new WindowsTimeServiceProbe()
        : (OperatingSystem.IsLinux() && Environment.Is64BitProcess) ? new LinuxAdjtimexProbe()
        : new UnsupportedProbe();

    public ClockDisciplineReading Probe()
    {
        try
        {
            return _inner.Probe();
        }
        catch (Exception ex)
        {
            return ClockDisciplineReading.Unknown($"probe threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    private sealed class UnsupportedProbe : IClockDisciplineProbe
    {
        public ClockDisciplineReading Probe() => ClockDisciplineReading.Unknown("no clock discipline probe for this platform");
    }
}

/// <summary>The result of running a child process for <see cref="WindowsTimeServiceProbe"/>.</summary>
internal readonly record struct CommandResult(bool Started, bool TimedOut, int ExitCode, string Output, string? Error);

/// <summary>
/// Windows: runs <c>w32tm /query /status /verbose</c> (no elevation needed; <c>/query /source</c> and <c>/query /configuration</c> are denied
/// without it) with a timeout and parses the English output. See the documentation page for why this source was chosen.
/// </summary>
/// <remarks>
/// Only what is positively recognized is accepted: the labels are English, so on a non-English Windows the output is not recognized and
/// the reading is a failure (<see cref="ClockQuality.System"/>, unknown). Numeric fields are used where they exist (leap indicator, stratum,
/// root delay, root dispersion, phase offset); the source name is text only.
/// </remarks>
public sealed class WindowsTimeServiceProbe : IClockDisciplineProbe
{
    internal static readonly TimeSpan s_defaultTimeout = TimeSpan.FromSeconds(5);
    private const int MaxOutputChars = 16_384;

    private readonly Func<TimeSpan, CommandResult> _run;
    private readonly TimeSpan _timeout;

    public WindowsTimeServiceProbe() : this(RunW32tm, s_defaultTimeout)
    {
    }

    internal WindowsTimeServiceProbe(Func<TimeSpan, CommandResult> run, TimeSpan timeout)
    {
        _run = run;
        _timeout = timeout;
    }

    public ClockDisciplineReading Probe()
    {
        try
        {
            CommandResult result = _run(_timeout);
            if (!result.Started)
            {
                return ClockDisciplineReading.Unknown($"could not run w32tm: {result.Error}");
            }
            if (result.TimedOut)
            {
                return ClockDisciplineReading.Unknown($"w32tm did not answer within {_timeout.TotalSeconds:0.#} s");
            }
            if (result.ExitCode != 0)
            {
                return ClockDisciplineReading.Unknown($"w32tm exited with code {result.ExitCode}: {Truncate(result.Error ?? result.Output)}");
            }
            return Parse(result.Output);
        }
        catch (Exception ex)
        {
            return ClockDisciplineReading.Unknown($"w32tm probe failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static string Truncate(string text)
    {
        string single = text.Trim().ReplaceLineEndings(" ");
        return single.Length <= 160 ? single : single[..160];
    }

    /// <summary>Interprets the text printed by <c>w32tm /query /status /verbose</c>. Pure; never throws.</summary>
    internal static ClockDisciplineReading Parse(string? output)
    {
        if (string.IsNullOrWhiteSpace(output) || output.Length > MaxOutputChars)
        {
            return ClockDisciplineReading.Unknown("w32tm printed nothing usable");
        }
        Dictionary<string, string> fields = [with(StringComparer.Ordinal)];
        foreach (string line in output.Split('\n'))
        {
            int colon = line.IndexOf(':');
            if (colon > 0)
            {
                fields.TryAdd(line[..colon].Trim(), line[(colon + 1)..].Trim());
            }
        }

        if (!fields.TryGetValue("Leap Indicator", out string? leapText) || !TryLeadingInt(leapText, out int leap) || leap is < 0 or > 3)
        {
            return ClockDisciplineReading.Unknown("w32tm output not recognized (expected English labels)");
        }
        if (!fields.TryGetValue("Source", out string? sourceText) || string.IsNullOrWhiteSpace(sourceText))
        {
            return ClockDisciplineReading.Unknown("w32tm output not recognized (no Source)");
        }
        string rawSource = sourceText;
        if (leap == 3)
        {
            return ClockDisciplineReading.FreeRunning($"w32time reports not synchronized (source {rawSource})");
        }

        if (!fields.TryGetValue("Stratum", out string? stratumText) || !TryLeadingInt(stratumText, out int stratum)
            || !TryParseSeconds(fields, "Root Delay", out double rootDelay)
            || !TryParseSeconds(fields, "Root Dispersion", out double rootDispersion)
            || !TryParseSeconds(fields, "Phase Offset", out double phaseOffset))
        {
            return ClockDisciplineReading.Unknown("w32tm output not recognized (numeric fields missing or unparseable)");
        }
        if (stratum is < 1 or > 15)
        {
            return ClockDisciplineReading.FreeRunning($"w32time reports synchronized but with invalid stratum {stratum} (source {rawSource})");
        }

        string source = NormalizeSource(rawSource);
        string compact = source.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        if (compact is "localcmosclock" or "free-runningsystemclock" || compact.StartsWith("localcmos", StringComparison.Ordinal))
        {
            return ClockDisciplineReading.FreeRunning($"w32time source is the local clock ({rawSource})");
        }
        if (compact.StartsWith("vmictimeprovider", StringComparison.Ordinal))
        {
            return ClockDisciplineReading.FreeRunning($"w32time follows the Hyper-V host (source {rawSource}); its accuracy is not reported");
        }

        // The conventional NTP bound is the root synchronization distance: root dispersion + half the root delay. The current
        // measured phase offset is added so a clock that is still being slewed in is not reported as better than it is.
        double errorSeconds = rootDispersion + (rootDelay / 2) + Math.Abs(phaseOffset);
        if (!double.IsFinite(errorSeconds) || errorSeconds < 0 || errorSeconds > 86_400)
        {
            return ClockDisciplineReading.Unknown($"w32tm reported an implausible error bound ({errorSeconds:0.###} s)");
        }
        TimeSpan error = TimeSpan.FromTicks((long)Math.Round(errorSeconds * TimeSpan.TicksPerSecond));
        string detail = $"w32time synchronized to {rawSource} (stratum {stratum}); error bound = root dispersion + root delay / 2 + |phase offset|, the service's own estimate";

        if (compact is "ptpprovider")
        {
            // UNVERIFIED: the exact Source text of a Windows PTP provider has not been observed; this is the only spelling accepted.
            return new ClockDisciplineReading(ClockQuality.PtpSoftware, error, "ptp:windows-provider", detail);
        }
        if (!IsPlausibleHost(source))
        {
            return ClockDisciplineReading.Unknown($"w32tm reported an unrecognized source '{Truncate(rawSource)}'");
        }
        return new ClockDisciplineReading(ClockQuality.Ntp, error, $"ntp:{source.ToLowerInvariant()}", detail);
    }

    /// <summary>Removes the provider flags ("time.example.com,0x8") and any peer detail ("|..."), and a trailing dot.</summary>
    internal static string NormalizeSource(string raw)
    {
        string source = raw;
        int cut = source.IndexOfAny([',', '|']);
        if (cut >= 0)
        {
            source = source[..cut];
        }
        return source.Trim().TrimEnd('.');
    }

    private static bool IsPlausibleHost(string host)
    {
        if (host.Length is 0 or > 253)
        {
            return false;
        }
        foreach (char c in host)
        {
            if (!(char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_' or ':' or '[' or ']'))
            {
                return false;
            }
        }
        return true;
    }

    private static bool TryLeadingInt(string text, out int value)
    {
        int end = 0;
        while (end < text.Length && char.IsAsciiDigit(text[end]))
        {
            end++;
        }
        value = 0;
        return end > 0 && int.TryParse(text.AsSpan(0, end), NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseSeconds(Dictionary<string, string> fields, string label, out double seconds)
    {
        seconds = 0;
        if (!fields.TryGetValue(label, out string? text) || !text.EndsWith('s'))
        {
            return false;
        }
        return double.TryParse(text.AsSpan(0, text.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out seconds) && double.IsFinite(seconds);
    }

    private static CommandResult RunW32tm(TimeSpan timeout)
    {
        try
        {
            string path = Path.Combine(Environment.SystemDirectory, "w32tm.exe");
            using Process process = new();
            process.StartInfo = new ProcessStartInfo(path, "/query /status /verbose")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            };
            process.Start();
            process.StandardInput.Close();
            Task<string> output = process.StandardOutput.ReadToEndAsync();
            Task<string> error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(timeout))
            {
                try
                {
                    process.Kill(true);
                }
                catch (Exception)
                {
                    // Already gone.
                }
                return new CommandResult(true, true, -1, string.Empty, null);
            }
            process.WaitForExit();
            Task.WaitAll([output, error], TimeSpan.FromSeconds(1));
            return new CommandResult(true, false, process.ExitCode, output.IsCompletedSuccessfully ? output.Result : string.Empty, error.IsCompletedSuccessfully ? error.Result : null);
        }
        catch (Exception ex)
        {
            return new CommandResult(false, false, -1, string.Empty, $"{ex.GetType().Name}: {ex.Message}");
        }
    }
}

/// <summary>The fields of the Linux kernel's <c>struct timex</c> (LP64 layout, 208 bytes), as filled by <c>adjtimex(2)</c>.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct LinuxTimex
{
    public uint Modes;
    public long Offset;
    public long Freq;
    public long MaxError;
    public long EstError;
    public int Status;
    public long Constant;
    public long Precision;
    public long Tolerance;
    public long TimeSec;
    public long TimeUsec;
    public long Tick;
    public long PpsFreq;
    public long Jitter;
    public int Shift;
    public long Stabil;
    public long JitCnt;
    public long CalCnt;
    public long ErrCnt;
    public long StbCnt;
    public int Tai;
    public int Pad0, Pad1, Pad2, Pad3, Pad4, Pad5, Pad6, Pad7, Pad8, Pad9, Pad10;
}

/// <summary>
/// Linux (64-bit): reads the kernel's NTP discipline state with <c>adjtimex(2)</c>, modes = 0 (read-only, no privileges). NOT RUN on
/// Linux when written: only <see cref="Interpret"/> is unit-tested.
/// </summary>
/// <remarks>
/// adjtimex cannot say which source disciplines the clock (and a PTP agent using phc2sys or ptp4l also ends up here), so a synchronized
/// clock is reported as <see cref="ClockQuality.Ntp"/> with an absolute clock id that names only this machine
/// (<c>ntp:unidentified:{machine id}</c>): it will never be treated as sharing a source with another machine until a source can be named.
/// A container whose seccomp profile blocks adjtimex (Docker's default) reads as a failure.
/// </remarks>
public sealed class LinuxAdjtimexProbe : IClockDisciplineProbe
{
    private const int TimeError = 5;
    private const int StaUnsync = 0x0040;
    private const long KernelMaxErrorMicroseconds = 16_000_000;

    private readonly Func<(int Result, int Errno, LinuxTimex Timex)> _read;

    public LinuxAdjtimexProbe() : this(ReadNative)
    {
    }

    internal LinuxAdjtimexProbe(Func<(int Result, int Errno, LinuxTimex Timex)> read)
    {
        _read = read;
    }

    public ClockDisciplineReading Probe()
    {
        try
        {
            (int result, int errno, LinuxTimex timex) = _read();
            return Interpret(result, errno, timex.Status, timex.MaxError);
        }
        catch (Exception ex)
        {
            return ClockDisciplineReading.Unknown($"adjtimex failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Interprets adjtimex output. The error is <c>maxerror</c> (microseconds), not <c>esterror</c>: maxerror is the kernel's maintained upper
    /// bound against the source (ntpd and chrony set it from the root distance and the kernel grows it by 500 ppm while no update arrives, so a
    /// dead agent shows a growing bound); esterror is a statistical estimate that can be optimistic. A bound is what a minimum separation needs.
    /// </summary>
    /// <param name="result">The return value: the clock state (0 = TIME_OK ... 5 = TIME_ERROR) or -1.</param>
    /// <param name="errno">errno if <paramref name="result"/> is -1.</param>
    /// <param name="status">timex.status (STA_* bits; STA_UNSYNC = 0x40).</param>
    /// <param name="maxErrorMicroseconds">timex.maxerror in microseconds.</param>
    internal static ClockDisciplineReading Interpret(int result, int errno, int status, long maxErrorMicroseconds)
    {
        if (result < 0)
        {
            return ClockDisciplineReading.Unknown($"adjtimex failed (errno {errno})");
        }
        if (result == TimeError || (status & StaUnsync) != 0)
        {
            return ClockDisciplineReading.FreeRunning($"kernel reports the clock unsynchronized (state {result}, status 0x{status:x})");
        }
        if (maxErrorMicroseconds < 0)
        {
            return ClockDisciplineReading.Unknown($"adjtimex reported a negative maxerror ({maxErrorMicroseconds})");
        }
        if (maxErrorMicroseconds >= KernelMaxErrorMicroseconds)
        {
            return ClockDisciplineReading.FreeRunning($"kernel reports the clock synchronized but maxerror is at its limit ({maxErrorMicroseconds} us)");
        }
        return new ClockDisciplineReading(
            ClockQuality.Ntp,
            TimeSpan.FromTicks(maxErrorMicroseconds * 10),
            $"ntp:unidentified:{MonotonicClock.MachineId}",
            $"kernel reports the clock synchronized; error bound = kernel maxerror {maxErrorMicroseconds} us (source not identifiable via adjtimex)");
    }

    private static (int Result, int Errno, LinuxTimex Timex) ReadNative()
    {
        if (!OperatingSystem.IsLinux() || !Environment.Is64BitProcess)
        {
            return (-1, 0, default);
        }
        LinuxTimex timex = default;
        int result = NativeLinux.AdjTimex(ref timex);
        return (result, result < 0 ? Marshal.GetLastPInvokeError() : 0, timex);
    }
}

[SupportedOSPlatform("linux")]
internal static partial class NativeLinux
{
    [LibraryImport("libc", EntryPoint = "adjtimex", SetLastError = true)]
    internal static partial int AdjTimex(ref LinuxTimex timex);
}
