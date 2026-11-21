#pragma warning disable IDE0130 // Namespace does not match folder: the clock types are published in GameshowPro.Common.Model; moving them to match the Clocks folder would break consumers.
namespace GameshowPro.Common.Model;
#pragma warning restore IDE0130

/// <summary>
/// The monotonic clock: <see cref="Stopwatch"/> timestamps expressed as a <see cref="TimeSpan"/> on a fixed origin, plus the identity of the
/// ruler they were read from.
/// </summary>
/// <remarks>
/// <para>
/// Origin: <see cref="TimeSpan.Zero"/> is Stopwatch timestamp 0. On Windows that is the QueryPerformanceCounter origin, which counts from
/// system boot including time asleep, is shared by all processes (Microsoft Learn, "Acquiring high-resolution time stamps": results are
/// consistent across processors, threads and processes; the count is "the total number of ticks that have occurred since the Windows
/// operating system was started"). On Linux, .NET's Stopwatch is clock_gettime(CLOCK_MONOTONIC) in nanoseconds (dotnet/runtime
/// src/native/minipal/time.c), "a nonsettable system-wide clock" whose zero is boot and which does not count suspend (clock_gettime(2)).
/// So two processes on one boot read comparable values on either OS; values from different boots or machines are not comparable,
/// which is what <see cref="Id"/> is for. Verified empirically on Windows (separate processes agree with Environment.TickCount64 to
/// within its resolution); not run on Linux.
/// </para>
/// <para>
/// Resolution of the <see cref="TimeSpan"/> is 100 ns (a Linux nanosecond timestamp is truncated to it). Conversions allocate nothing and
/// are thread-safe (pure functions of their arguments). Identity is resolved once, lazily, on first use of <see cref="Id"/>,
/// <see cref="MachineId"/> or the verification flags, and never throws.
/// </para>
/// </remarks>
public static class MonotonicClock
{
    private const long TicksPerSecond = TimeSpan.TicksPerSecond;

    /// <summary>The timestamp frequency (ticks per second) of <see cref="GetTimestamp"/>.</summary>
    public static long Frequency => Stopwatch.Frequency;

    /// <summary>The current monotonic timestamp, in units of 1 / <see cref="Frequency"/> seconds. Cheap enough for an interrupt-time capture path.</summary>
    public static long GetTimestamp() => Stopwatch.GetTimestamp();

    /// <summary>The current monotonic time on the fixed origin.</summary>
    public static TimeSpan Now => FromTimestamp(Stopwatch.GetTimestamp());

    /// <summary>Converts a <see cref="GetTimestamp"/> value to a <see cref="TimeSpan"/> on the fixed origin.</summary>
    public static TimeSpan FromTimestamp(long timestamp) => new(TimestampToTicks(timestamp, Stopwatch.Frequency));

    /// <summary>Converts a timestamp counted at <paramref name="frequency"/> ticks per second to a <see cref="TimeSpan"/> on the fixed origin.</summary>
    public static TimeSpan FromTimestamp(long timestamp, long frequency) => new(TimestampToTicks(timestamp, frequency));

    /// <summary>Converts a time on the fixed origin back to a <see cref="GetTimestamp"/> value.</summary>
    public static long ToTimestamp(TimeSpan time) => TicksToTimestamp(time.Ticks, Stopwatch.Frequency);

    /// <summary>Converts a time on the fixed origin to a timestamp counted at <paramref name="frequency"/> ticks per second.</summary>
    public static long ToTimestamp(TimeSpan time, long frequency) => TicksToTimestamp(time.Ticks, frequency);

    /// <summary>
    /// Names the monotonic ruler: this machine and this boot. Equal ids mean <see cref="FromTimestamp(long)"/> values are directly
    /// subtractable. Formats: <c>win:{machine guid}:boot{boot count}</c>, <c>linux:{machine-id}:{boot_id}</c>,
    /// <c>linux:boot:{boot_id}</c> (no readable machine-id), or <c>proc-random:{guid}</c> when the identity could not be read
    /// (see <see cref="IsIdVerified"/>).
    /// </summary>
    public static string Id => IdentityHolder.s_identity.MonotonicId;

    /// <summary>False when <see cref="Id"/> is the random per-process fallback: it never collides with another machine or process, but nothing else can share it.</summary>
    public static bool IsIdVerified => IdentityHolder.s_identity.MonotonicIdVerified;

    /// <summary>Names this machine, independent of boot (the owner of the machine's own free-running wall clock). Same fallback rules as <see cref="Id"/>.</summary>
    public static string MachineId => IdentityHolder.s_identity.MachineId;

    /// <summary>False when <see cref="MachineId"/> is the random per-process fallback.</summary>
    public static bool IsMachineIdVerified => IdentityHolder.s_identity.MachineIdVerified;

    internal static long TimestampToTicks(long timestamp, long frequency)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequency);
        if (frequency == TicksPerSecond)
        {
            return timestamp;
        }
        if (frequency == 1_000_000_000L)
        {
            return timestamp / 100;
        }
        return (long)((Int128)timestamp * TicksPerSecond / frequency);
    }

    internal static long TicksToTimestamp(long ticks, long frequency)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequency);
        if (frequency == TicksPerSecond)
        {
            return ticks;
        }
        if (frequency == 1_000_000_000L)
        {
            return ticks * 100;
        }
        return (long)((Int128)ticks * frequency / TicksPerSecond);
    }

    private static class IdentityHolder
    {
        internal static readonly MachineIdentity s_identity = MachineIdentity.Resolve();
    }
}

/// <summary>The machine and boot identity strings, and whether they were really read from the operating system.</summary>
internal readonly record struct MachineIdentity(string MachineId, string MonotonicId, bool MachineIdVerified, bool MonotonicIdVerified)
{
    /// <summary>Reads the identity without elevated rights. Never throws; anything unreadable falls back to a random per-process value.</summary>
    internal static MachineIdentity Resolve()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                ReadWindows(out string? machineGuid, out int? bootId);
                return FromWindows(machineGuid, bootId);
            }
            if (OperatingSystem.IsLinux())
            {
                return FromLinux(ReadSmallFile("/etc/machine-id") ?? ReadSmallFile("/var/lib/dbus/machine-id"), ReadSmallFile("/proc/sys/kernel/random/boot_id"));
            }
        }
        catch (Exception)
        {
            // Fall through to the random identity.
        }
        return FromLinux(null, null);
    }

    internal static MachineIdentity FromWindows(string? machineGuid, int? bootId)
    {
        string? machine = NormalizeGuid(machineGuid);
        if (machine is null)
        {
            return Fallback();
        }
        string machineId = $"win:{machine}";
        if (bootId is null)
        {
            return new MachineIdentity(machineId, RandomId(), true, false);
        }
        return new MachineIdentity(machineId, $"{machineId}:boot{bootId.Value.ToString(CultureInfo.InvariantCulture)}", true, true);
    }

    /// <summary>The boot_id alone is unique per boot and kernel, so a missing machine-id loses only the machine identity.</summary>
    internal static MachineIdentity FromLinux(string? machineIdText, string? bootIdText)
    {
        string? machine = NormalizeGuid(machineIdText);
        string? boot = NormalizeGuid(bootIdText);
        if (boot is null)
        {
            return machine is null ? Fallback() : new MachineIdentity($"linux:{machine}", RandomId(), true, false);
        }
        return machine is null
            ? new MachineIdentity(RandomId(), $"linux:boot:{boot}", false, true)
            : new MachineIdentity($"linux:{machine}", $"linux:{machine}:{boot}", true, true);
    }

    private static MachineIdentity Fallback() => new(RandomId(), RandomId(), false, false);

    private static string RandomId() => $"proc-random:{s_processRandom.ToString("N", CultureInfo.InvariantCulture)}";

    private static readonly Guid s_processRandom = Guid.NewGuid();

    private static string? NormalizeGuid(string? text)
    {
        string? trimmed = text?.Trim();
        if (!string.IsNullOrEmpty(trimmed) && Guid.TryParse(trimmed, out Guid guid) && guid != Guid.Empty)
        {
            return guid.ToString("D", CultureInfo.InvariantCulture);
        }
        return null;
    }

    private static string? ReadSmallFile(string path)
    {
        try
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1);
            byte[] buffer = new byte[128];
            int read = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, read);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Machine GUID: HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid (readable by any user). Boot count: the DWORD BootId under
    /// HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters, incremented each boot (readable by any user;
    /// undocumented but long relied on by forensic tools). If prefetching were disabled and the count stopped incrementing, two boots
    /// of one machine would share an identity; the machine GUID still prevents any collision between machines.
    /// </summary>
    private static void ReadWindows(out string? machineGuid, out int? bootId)
    {
        machineGuid = null;
        bootId = null;
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        try
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? crypto = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            machineGuid = crypto?.GetValue("MachineGuid") as string;
            using RegistryKey? prefetch = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters");
            bootId = prefetch?.GetValue("BootId") as int?;
        }
        catch (Exception)
        {
            // Leave whatever was read; the caller falls back for the rest.
        }
    }
}
