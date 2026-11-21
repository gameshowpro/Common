#pragma warning disable IDE0130 // Namespace does not match folder: the clock types are published in GameshowPro.Common.Model; moving them to match the Clocks folder would break consumers.
namespace GameshowPro.Common.Model;
#pragma warning restore IDE0130

/// <summary>The two raw clocks <see cref="AbsoluteClockMap"/> reads. Injectable so tests can drive both without sleeping.</summary>
public interface ITimeSource
{
    /// <summary>Ticks per second of <see cref="GetTimestamp"/>.</summary>
    long Frequency { get; }

    /// <summary>The monotonic timestamp (<see cref="Stopwatch.GetTimestamp"/> for the real source).</summary>
    long GetTimestamp();

    /// <summary>The wall clock as UTC ticks (<see cref="DateTime.UtcNow"/> for the real source).</summary>
    long GetUtcTicks();
}

/// <summary>The real clocks: <see cref="Stopwatch"/> and <see cref="DateTime.UtcNow"/>.</summary>
public sealed class SystemTimeSource : ITimeSource
{
    public static readonly SystemTimeSource Instance = new();

    private SystemTimeSource()
    {
    }

    public long Frequency => Stopwatch.Frequency;

    public long GetTimestamp() => Stopwatch.GetTimestamp();

    public long GetUtcTicks() => DateTime.UtcNow.Ticks;
}

/// <summary>Tuning for <see cref="AbsoluteClockMap"/>.</summary>
public sealed record AbsoluteClockMapOptions
{
    /// <summary>
    /// How often the timer re-samples the offset. 1 s: a slewing agent moves the offset by at most hundreds of microseconds per second, so a
    /// step is caught within a second, and each sample costs about a microsecond.
    /// </summary>
    public TimeSpan SampleInterval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How far the offset may move, beyond what slewing and measurement noise explain, before the change is called a step. 1 ms: steps
    /// below that are indistinguishable from scheduling noise in the bracketing reads, and small against the clocks' other errors.
    /// </summary>
    public TimeSpan StepThreshold { get; init; } = TimeSpan.FromMilliseconds(1);

    /// <summary>
    /// The largest rate at which the offset may drift or be slewed without being called a step. 500 ppm: the limit of the Linux kernel's
    /// NTP discipline (and of ntpd); crystal drift is tens of ppm. An agent configured to slew faster (chrony allows far more, Windows
    /// can slew by percent) will have its fastest corrections reported as steps, which is the conservative direction.
    /// </summary>
    public double MaxSlewRate { get; init; } = 0.0005;

    /// <summary>Bracketed reads per sample; the one with the narrowest bracket wins. Anything that preempts the thread widens a bracket and loses.</summary>
    public int ReadsPerSample { get; init; } = 8;

    /// <summary>How often the timer polls the <see cref="IClockDisciplineProbe"/> (it can start a child process): 60 s.</summary>
    public TimeSpan DisciplineInterval { get; init; } = TimeSpan.FromSeconds(60);
}

/// <summary>Raised when <see cref="AbsoluteClockMap"/> detects that the wall clock jumped against the monotonic clock.</summary>
public sealed class ClockStepEventArgs(ClockStep step) : EventArgs
{
    public ClockStep Step { get; } = step;
}

/// <summary>
/// Maps monotonic timestamps to UTC with an offset sampled periodically, so that an edge stamped on the monotonic clock in the capture
/// path is converted at report time and a step in the wall clock cannot reorder edges.
/// </summary>
/// <remarks>
/// <para>Offset = wall clock minus monotonic time, measured with bracketed reads (monotonic, wall, monotonic: the wall reading is taken to
/// be at the midpoint, with half the bracket as its uncertainty) and the best of several taken. Each sample replaces the offset in force
/// (so slow slewing is followed); if it differs from the previous one by more than the allowed drift (see
/// <see cref="AbsoluteClockMapOptions"/>) it is also reported as a step: <see cref="StepDetected"/> and <see cref="LastStep"/>.</para>
/// <para>Conversion (<see cref="ToUtcTicks(long)"/> and friends) is one volatile read of an immutable snapshot plus integer arithmetic: no locks,
/// no allocation (<see cref="ToUtc"/> returns a struct), no clock reads, safe from any thread. A timestamp captured before a step but converted
/// after the next sample is mapped with the new offset (the wall clock as it now reads); history of offsets is not kept.</para>
/// <para>Sampling and discipline probing happen only in <see cref="Sample"/> and <see cref="PollDiscipline"/>, called on demand or by
/// the timer started with <see cref="StartSampling"/>; never by a conversion.</para>
/// </remarks>
public sealed class AbsoluteClockMap : IDisposable
{
    private sealed record OffsetSnapshot(long OffsetTicks, long MonotonicTicks, long UncertaintyTicks);

    private readonly ITimeSource _source;
    private readonly IClockDisciplineProbe _probe;
    private readonly AbsoluteClockMapOptions _options;
    private readonly Lock _sync = new();
    private readonly long _frequency;
    private volatile OffsetSnapshot _current;
    private volatile ClockStep? _lastStep;
    private volatile ClockDisciplineReading? _reading;
    private long _updatedAtUtcTicks;
    private long _lastProbeTimestamp;
    private Timer? _timer;
    private volatile bool _disposed;

    /// <param name="options">Null = defaults.</param>
    /// <param name="source">Null = the real clocks.</param>
    /// <param name="probe">Null = <see cref="SystemClockDisciplineProbe"/>. Probing happens only in <see cref="PollDiscipline"/> or on the timer, not in the constructor.</param>
    public AbsoluteClockMap(AbsoluteClockMapOptions? options = null, ITimeSource? source = null, IClockDisciplineProbe? probe = null)
    {
        _options = options ?? new AbsoluteClockMapOptions();
        ArgumentOutOfRangeException.ThrowIfLessThan(_options.ReadsPerSample, 1);
        _source = source ?? SystemTimeSource.Instance;
        _probe = probe ?? SystemClockDisciplineProbe.Instance;
        _frequency = _source.Frequency;
        _current = Measure();
        _updatedAtUtcTicks = _source.GetUtcTicks();
        _lastProbeTimestamp = long.MinValue;
    }

    /// <summary>Raised on the sampling thread, outside any lock, when a step is detected. Handlers must not block.</summary>
    public event EventHandler<ClockStepEventArgs>? StepDetected;

    /// <summary>The last step detected, or null.</summary>
    public ClockStep? LastStep => _lastStep;

    /// <summary>The offset (wall clock minus monotonic time on the fixed origin) in force.</summary>
    public TimeSpan Offset => new(_current.OffsetTicks);

    /// <summary>Half the bracket width of the sample in force: the bound on how well the offset was measured.</summary>
    public TimeSpan Uncertainty => new(_current.UncertaintyTicks);

    /// <summary>The error from the last failed timer callback (a throwing <see cref="StepDetected"/> handler), for logging; null if none.</summary>
    public string? LastTimerError { get; private set; }

    /// <summary>Converts a monotonic timestamp (<see cref="MonotonicClock.GetTimestamp"/> or the source's) to UTC ticks with the offset in force.</summary>
    public long ToUtcTicks(long timestamp) => MonotonicClock.TimestampToTicks(timestamp, _frequency) + _current.OffsetTicks;

    /// <summary>Converts a monotonic time on the fixed origin to UTC ticks.</summary>
    public long ToUtcTicks(TimeSpan monotonic) => monotonic.Ticks + _current.OffsetTicks;

    /// <summary>Converts a monotonic timestamp to UTC.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The result is outside the range of <see cref="DateTime"/>.</exception>
    public DateTime ToUtc(long timestamp) => new(ToUtcTicks(timestamp), DateTimeKind.Utc);

    /// <summary>Converts a UTC instant back to a monotonic timestamp with the offset in force.</summary>
    public long ToTimestamp(DateTime utc) => MonotonicClock.TicksToTimestamp(utc.ToUniversalTime().Ticks - _current.OffsetTicks, _frequency);

    /// <summary>
    /// Takes a new sample and makes it the offset in force. Returns the step if the offset moved by more than the allowed drift, otherwise null.
    /// Safe to call from any thread; samples are serialized.
    /// </summary>
    public ClockStep? Sample()
    {
        ClockStep? step = null;
        lock (_sync)
        {
            OffsetSnapshot previous = _current;
            OffsetSnapshot next = Measure();
            long elapsed = Math.Max(0, next.MonotonicTicks - previous.MonotonicTicks);
            long allowed = _options.StepThreshold.Ticks + (long)(_options.MaxSlewRate * elapsed) + previous.UncertaintyTicks + next.UncertaintyTicks;
            long delta = next.OffsetTicks - previous.OffsetTicks;
            if (Math.Abs(delta) > allowed)
            {
                step = new ClockStep(new DateTimeOffset(next.MonotonicTicks + next.OffsetTicks, TimeSpan.Zero), new TimeSpan(delta));
                _lastStep = step;
            }
            _current = next;
            Volatile.Write(ref _updatedAtUtcTicks, next.MonotonicTicks + next.OffsetTicks);
        }
        if (step is not null)
        {
            StepDetected?.Invoke(this, new ClockStepEventArgs(step));
        }
        return step;
    }

    /// <summary>Asks the probe about the wall clock's discipline and keeps the answer for <see cref="GetStatus"/>. Never throws; may be slow.</summary>
    public ClockDisciplineReading PollDiscipline()
    {
        ClockDisciplineReading reading;
        try
        {
            reading = _probe.Probe();
        }
        catch (Exception ex)
        {
            reading = ClockDisciplineReading.Unknown($"probe threw {ex.GetType().Name}: {ex.Message}");
        }
        _reading = reading;
        Volatile.Write(ref _lastProbeTimestamp, _source.GetTimestamp());
        Volatile.Write(ref _updatedAtUtcTicks, _source.GetUtcTicks());
        return reading;
    }

    /// <summary>
    /// The status to publish: monotonic identity, the discipline probe's latest reading (System with unknown error until the first poll),
    /// the last step. Allocates; not for the conversion path.
    /// </summary>
    public ClockStatus GetStatus()
    {
        ClockDisciplineReading? reading = _reading;
        DateTimeOffset updatedAt = new(Volatile.Read(ref _updatedAtUtcTicks), TimeSpan.Zero);
        return reading is null
            ? new ClockStatus(MonotonicClock.Id, ClockDisciplineReading.FreeRunningId, ClockQuality.System, null, _lastStep, updatedAt, "clock discipline not probed yet")
            : new ClockStatus(MonotonicClock.Id, reading.AbsoluteClockId, reading.Quality, reading.EstimatedError, _lastStep, updatedAt, reading.Detail);
    }

    /// <summary>
    /// Starts a timer that samples every <see cref="AbsoluteClockMapOptions.SampleInterval"/> and polls the probe every
    /// <see cref="AbsoluteClockMapOptions.DisciplineInterval"/> (immediately the first time), on the thread pool. Idempotent.
    /// </summary>
    public void StartSampling()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _timer ??= new Timer(OnTimer, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }
    }

    private void OnTimer(object? state)
    {
        try
        {
            if (_disposed)
            {
                return;
            }
            Sample();
            long last = Volatile.Read(ref _lastProbeTimestamp);
            if (last == long.MinValue || _source.GetTimestamp() - last >= _options.DisciplineInterval.TotalSeconds * _frequency)
            {
                PollDiscipline();
            }
        }
        catch (Exception ex)
        {
            LastTimerError = $"{ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            lock (_sync)
            {
                if (!_disposed)
                {
                    _timer?.Change(_options.SampleInterval, Timeout.InfiniteTimeSpan);
                }
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _disposed = true;
            _timer?.Dispose();
            _timer = null;
        }
    }

    private OffsetSnapshot Measure()
    {
        long bestWidth = long.MaxValue;
        long bestOffset = 0;
        long bestMid = 0;
        for (int i = 0; i < _options.ReadsPerSample; i++)
        {
            long before = _source.GetTimestamp();
            long utc = _source.GetUtcTicks();
            long after = _source.GetTimestamp();
            long width = after - before;
            if (width < bestWidth)
            {
                bestWidth = width;
                bestMid = MonotonicClock.TimestampToTicks(before + (width / 2), _frequency);
                bestOffset = utc - bestMid;
            }
        }
        long widthTicks = MonotonicClock.TimestampToTicks(Math.Max(0, bestWidth), _frequency);
        return new OffsetSnapshot(bestOffset, bestMid, (widthTicks + 1) / 2);
    }
}
