namespace GameshowPro.Common.Test;

#if WPF
// The Windows configurations target a Windows-only TFM, so every test class is platform-specific (MSTEST0084).
[OSCondition(OperatingSystems.Windows)]
#endif
[TestClass]
public class TestClocks
{
    // ---------------------------------------------------------------- helpers

    private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed class FakeTimeSource(long frequency) : ITimeSource
    {
        public long Timestamp;
        public long UtcTicks;
        public long Frequency { get; } = frequency;
        public long GetTimestamp() => Timestamp;
        public long GetUtcTicks() => UtcTicks;
    }

    private sealed class FakeProbe(ClockDisciplineReading? reading, Exception? toThrow = null) : IClockDisciplineProbe
    {
        public int Calls;
        public ClockDisciplineReading Probe()
        {
            Calls++;
            if (toThrow is not null)
            {
                throw toThrow;
            }
            return reading!;
        }
    }

    private static ClockStatus Status(string mono, string abs, ClockQuality quality, TimeSpan? error)
        => new(mono, abs, quality, error);

    private static OrderingParticipant P(double? resolutionMs, ClockStatus clock)
        => new(resolutionMs is null ? null : TimeSpan.FromMilliseconds(resolutionMs.Value), clock);

    // ---------------------------------------------------------------- OrderingSeparation

    [TestMethod]
    public void Separation_SameMonotonicClock_IsLargerResolution_EvenWithQualityNoneAndUnknownError()
    {
        ClockStatus a = Status("m1", "system:A", ClockQuality.None, null);
        ClockStatus b = Status("m1", "ntp:x", ClockQuality.PtpHardware, TimeSpan.FromSeconds(9));
        OrderingSeparation result = OrderingSeparation.Minimum(P(0.2, a), P(1, b));
        Assert.AreEqual(OrderingRule.SameMonotonicClock, result.Rule);
        Assert.AreEqual(TimeSpan.FromMilliseconds(1), result.Separation);
        Assert.IsNull(result.Reason);
        Assert.IsTrue(result.IsComparable);
    }

    [TestMethod]
    public void Separation_SharedTimeSource_IsResolutionPlusBothErrors()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.FromMilliseconds(3));
        ClockStatus b = Status("m2", "ntp:x", ClockQuality.PtpSoftware, TimeSpan.FromMilliseconds(5));
        OrderingSeparation result = OrderingSeparation.Minimum(P(2, a), P(4, b));
        Assert.AreEqual(OrderingRule.SharedTimeSource, result.Rule);
        Assert.AreEqual(TimeSpan.FromMilliseconds(12), result.Separation);
    }

    [TestMethod]
    public void Separation_SharedTimeSource_SaturatesInsteadOfOverflowing()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.MaxValue);
        ClockStatus b = Status("m2", "ntp:x", ClockQuality.Ntp, TimeSpan.MaxValue);
        Assert.AreEqual(TimeSpan.MaxValue, OrderingSeparation.Minimum(P(1, a), P(1, b)).Separation);
    }

    [TestMethod]
    public void Separation_DifferentSources_NotComparable()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.Zero);
        ClockStatus b = Status("m2", "ntp:y", ClockQuality.Ntp, TimeSpan.Zero);
        OrderingSeparation result = OrderingSeparation.Minimum(P(1, a), P(1, b));
        Assert.AreEqual(OrderingRule.NotComparable, result.Rule);
        Assert.IsNull(result.Separation);
        Assert.IsNotNull(result.Reason);
        Assert.Contains("different time sources", result.Reason);
    }

    [TestMethod]
    public void Separation_QualityNone_NotComparable()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.None, TimeSpan.Zero);
        ClockStatus b = Status("m2", "ntp:x", ClockQuality.Ntp, TimeSpan.Zero);
        OrderingSeparation result = OrderingSeparation.Minimum(P(1, a), P(1, b));
        Assert.AreEqual(OrderingRule.NotComparable, result.Rule);
        Assert.IsNotNull(result.Reason);
        Assert.Contains("quality None", result.Reason);
    }

    [TestMethod]
    public void Separation_UnknownError_NotComparable_SoSystemQualityAcrossMachinesIsNot()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.Zero);
        ClockStatus b = Status("m2", "ntp:x", ClockQuality.Ntp, null);
        OrderingSeparation result = OrderingSeparation.Minimum(P(1, a), P(1, b));
        Assert.AreEqual(OrderingRule.NotComparable, result.Rule);
        Assert.IsNotNull(result.Reason);
        Assert.Contains("unknown", result.Reason);

        // A free-running clock reads System with unknown error, and its id names only its own machine.
        ClockStatus s1 = Status("m1", "system:A", ClockQuality.System, null);
        ClockStatus s2 = Status("m2", "system:B", ClockQuality.System, null);
        Assert.AreEqual(OrderingRule.NotComparable, OrderingSeparation.Minimum(P(1, s1), P(1, s2)).Rule);
    }

    [TestMethod]
    public void Separation_UnknownOrNegativeResolution_NotComparable_EvenOnTheSameMonotonicClock()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.Zero);
        ClockStatus b = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.Zero);
        Assert.AreEqual(OrderingRule.NotComparable, OrderingSeparation.Minimum(P(null, a), P(1, b)).Rule);
        Assert.AreEqual(OrderingRule.NotComparable, OrderingSeparation.Minimum(P(1, a), P(null, b)).Rule);
        Assert.AreEqual(OrderingRule.NotComparable, OrderingSeparation.Minimum(P(-1, a), P(1, b)).Rule);
    }

    [TestMethod]
    public void Separation_NegativeError_NotComparable_AndEmptyIdsNeverMatch()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.FromMilliseconds(-1));
        ClockStatus b = Status("m2", "ntp:x", ClockQuality.Ntp, TimeSpan.Zero);
        Assert.AreEqual(OrderingRule.NotComparable, OrderingSeparation.Minimum(P(1, a), P(1, b)).Rule);
        ClockStatus e1 = Status("", "", ClockQuality.Ntp, TimeSpan.Zero);
        ClockStatus e2 = Status("", "", ClockQuality.Ntp, TimeSpan.Zero);
        Assert.AreEqual(OrderingRule.NotComparable, OrderingSeparation.Minimum(P(1, e1), P(1, e2)).Rule);
    }

    [TestMethod]
    public void Separation_IsSymmetric()
    {
        ClockStatus a = Status("m1", "ntp:x", ClockQuality.Ntp, TimeSpan.FromMilliseconds(3));
        ClockStatus b = Status("m2", "ntp:x", ClockQuality.Ntp, TimeSpan.FromMilliseconds(5));
        Assert.AreEqual(OrderingSeparation.Minimum(P(2, a), P(4, b)), OrderingSeparation.Minimum(P(4, b), P(2, a)));
    }

    // ---------------------------------------------------------------- MonotonicClock

    [TestMethod]
    [DataRow(10_000_000L)]
    [DataRow(1_000_000_000L)]
    [DataRow(3_125_000L)]
    [DataRow(24_000_000L)]
    public void MonotonicClock_Conversion_RoundTrips(long frequency)
    {
        foreach (long ts in new long[] { 0, 1, 12_345, 9_876_543_210, 40_000_000_000_000 })
        {
            TimeSpan time = MonotonicClock.FromTimestamp(ts, frequency);
            long back = MonotonicClock.ToTimestamp(time, frequency);
            // Truncation to 100 ns loses at most one TimeSpan tick of precision.
            double toleranceTimestamps = Math.Max(1, frequency / 10_000_000.0);
            Assert.IsLessThanOrEqualTo(toleranceTimestamps, Math.Abs(back - ts), $"ts={ts} f={frequency}");
            double toleranceTicks = Math.Max(1, 10_000_000.0 / frequency);
            Assert.IsLessThanOrEqualTo(toleranceTicks, Math.Abs((time - MonotonicClock.FromTimestamp(back, frequency)).Ticks));
        }
    }

    [TestMethod]
    public void MonotonicClock_Conversion_KnownValues()
    {
        Assert.AreEqual(TimeSpan.FromSeconds(2), MonotonicClock.FromTimestamp(2_000_000_000, 1_000_000_000));
        Assert.AreEqual(TimeSpan.FromSeconds(2), MonotonicClock.FromTimestamp(6_250_000, 3_125_000));
        Assert.AreEqual(TimeSpan.FromTicks(1), MonotonicClock.FromTimestamp(100, 1_000_000_000));
        Assert.AreEqual(1_000_000_000L, MonotonicClock.ToTimestamp(TimeSpan.FromSeconds(1), 1_000_000_000));
        Assert.Throws<ArgumentOutOfRangeException>(() => MonotonicClock.FromTimestamp(1, 0));
    }

    [TestMethod]
    public void MonotonicClock_Now_AdvancesAndMatchesStopwatchOrigin()
    {
        TimeSpan a = MonotonicClock.Now;
        TimeSpan b = MonotonicClock.Now;
        Assert.IsGreaterThanOrEqualTo(a, b);
        long ts = Stopwatch.GetTimestamp();
        TimeSpan expected = Stopwatch.GetElapsedTime(0, ts);
        Assert.IsLessThanOrEqualTo(1L, Math.Abs((MonotonicClock.FromTimestamp(ts) - expected).Ticks));
    }

    [TestMethod]
    public void MonotonicClock_Identity_IsStableAndWellFormed()
    {
        string id = MonotonicClock.Id;
        Assert.AreEqual(id, MonotonicClock.Id);
        Assert.IsFalse(string.IsNullOrWhiteSpace(id));
        Assert.IsFalse(string.IsNullOrWhiteSpace(MonotonicClock.MachineId));
        if (MonotonicClock.IsIdVerified)
        {
            Assert.IsTrue(id.StartsWith("win:", StringComparison.Ordinal) || id.StartsWith("linux:", StringComparison.Ordinal), id);
        }
        else
        {
            Assert.StartsWith("proc-random:", id);
        }
        Console.WriteLine($"MonotonicClock.Id={id} verified={MonotonicClock.IsIdVerified}; MachineId={MonotonicClock.MachineId} verified={MonotonicClock.IsMachineIdVerified}");
    }

    [TestMethod]
    public void MonotonicClock_Identity_StableAcrossThreads()
    {
        string[] ids = new string[16];
        Parallel.For(0, ids.Length, i => ids[i] = MonotonicClock.Id);
        Assert.HasCount(1, ids.Distinct());
    }

    [TestMethod]
    public void MachineIdentity_Windows_Format()
    {
        MachineIdentity id = MachineIdentity.FromWindows("{1B2C3D4E-0000-1111-2222-333344445555}", 169);
        Assert.AreEqual("win:1b2c3d4e-0000-1111-2222-333344445555", id.MachineId);
        Assert.AreEqual("win:1b2c3d4e-0000-1111-2222-333344445555:boot169", id.MonotonicId);
        Assert.IsTrue(id.MachineIdVerified && id.MonotonicIdVerified);
    }

    [TestMethod]
    public void MachineIdentity_Windows_MissingBootCount_KeepsMachineButRandomBoot()
    {
        MachineIdentity id = MachineIdentity.FromWindows("1b2c3d4e-0000-1111-2222-333344445555", null);
        Assert.IsTrue(id.MachineIdVerified);
        Assert.IsFalse(id.MonotonicIdVerified);
        Assert.StartsWith("proc-random:", id.MonotonicId);
    }

    [TestMethod]
    public void MachineIdentity_Windows_UnreadableOrGarbage_FallsBackToRandom()
    {
        foreach (string? bad in new string?[] { null, "", "not a guid", "00000000-0000-0000-0000-000000000000" })
        {
            MachineIdentity id = MachineIdentity.FromWindows(bad, 5);
            Assert.IsFalse(id.MachineIdVerified);
            Assert.IsFalse(id.MonotonicIdVerified);
            Assert.StartsWith("proc-random:", id.MachineId);
            Assert.StartsWith("proc-random:", id.MonotonicId);
        }
    }

    [TestMethod]
    public void MachineIdentity_Linux_Format()
    {
        MachineIdentity id = MachineIdentity.FromLinux("0123456789abcdef0123456789abcdef\n", "11111111-2222-3333-4444-555555555555\n");
        Assert.AreEqual("linux:01234567-89ab-cdef-0123-456789abcdef", id.MachineId);
        Assert.AreEqual("linux:01234567-89ab-cdef-0123-456789abcdef:11111111-2222-3333-4444-555555555555", id.MonotonicId);
        Assert.IsTrue(id.MachineIdVerified && id.MonotonicIdVerified);
    }

    [TestMethod]
    public void MachineIdentity_Linux_Partial_AndFallback()
    {
        MachineIdentity noMachine = MachineIdentity.FromLinux(null, "11111111-2222-3333-4444-555555555555");
        Assert.AreEqual("linux:boot:11111111-2222-3333-4444-555555555555", noMachine.MonotonicId);
        Assert.IsTrue(noMachine.MonotonicIdVerified);
        Assert.IsFalse(noMachine.MachineIdVerified);

        MachineIdentity noBoot = MachineIdentity.FromLinux("0123456789abcdef0123456789abcdef", null);
        Assert.IsTrue(noBoot.MachineIdVerified);
        Assert.IsFalse(noBoot.MonotonicIdVerified);

        MachineIdentity neither = MachineIdentity.FromLinux(null, "garbage");
        Assert.StartsWith("proc-random:", neither.MonotonicId);
        Assert.StartsWith("proc-random:", neither.MachineId);
    }

    // ---------------------------------------------------------------- AbsoluteClockMap

    private static readonly long s_utc0 = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc).Ticks;

    private static FakeTimeSource NewSource(long frequency = 1_000_000_000) => new(frequency) { Timestamp = 5_000_000_000, UtcTicks = s_utc0 };

    [TestMethod]
    public void Map_Converts_PastTimestamps_WithOffsetInForce()
    {
        FakeTimeSource source = NewSource();
        using AbsoluteClockMap map = new(source: source, probe: new FakeProbe(null));
        // Timestamp 5 s <-> 12:00:00 UTC. An edge 2 ms earlier:
        DateTime utc = map.ToUtc(5_000_000_000 - 2_000_000);
        Assert.AreEqual(new DateTime(s_utc0 - 20_000, DateTimeKind.Utc), utc);
        Assert.AreEqual(DateTimeKind.Utc, utc.Kind);
        Assert.AreEqual(s_utc0 - (5_000_000_000L / 100), map.Offset.Ticks);
        Assert.AreEqual(TimeSpan.Zero, map.Uncertainty);
    }

    [TestMethod]
    public void Map_Conversion_RoundTrips()
    {
        FakeTimeSource source = NewSource(10_000_000);
        source.Timestamp = 123_456_789;
        using AbsoluteClockMap map = new(source: source, probe: new FakeProbe(null));
        foreach (long ts in new long[] { 0, 100, 123_456_789, 987_654_321 })
        {
            Assert.AreEqual(ts, map.ToTimestamp(map.ToUtc(ts)));
            Assert.AreEqual(map.ToUtcTicks(ts), map.ToUtcTicks(MonotonicClock.FromTimestamp(ts, 10_000_000)));
        }
    }

    [TestMethod]
    public void Map_BracketedRead_PicksNarrowestBracket_AndUsesMidpoint()
    {
        BracketSource source = new();
        using AbsoluteClockMap map = new(new AbsoluteClockMapOptions { ReadsPerSample = 3 }, source, new FakeProbe(null));
        // Bracket widths 1000, 100, 500 timestamp ticks (ns): the second wins; midpoint is 50 ns after its start.
        Assert.AreEqual(TimeSpan.FromTicks(1), map.Uncertainty);
        Assert.AreEqual(s_utc0 - (2_000_000 + 50) / 100, map.Offset.Ticks);
    }

    private sealed class BracketSource : ITimeSource
    {
        // Each reads sequence: before, utc, after. Widths 1000, 100, 500 ns; utc always s_utc0.
        private static readonly long[] s_timestamps = [1_000_000, 1_001_000, 2_000_000, 2_000_100, 3_000_000, 3_000_500];
        private int _index;
        public long Frequency => 1_000_000_000;
        public long GetTimestamp() => s_timestamps[_index++ % s_timestamps.Length];
        public long GetUtcTicks() => s_utc0;
    }

    [TestMethod]
    public void Map_DetectsAStep_RaisesEvent_AndKeepsConvertingWithNewOffset()
    {
        FakeTimeSource source = NewSource();
        using AbsoluteClockMap map = new(source: source, probe: new FakeProbe(null));
        List<ClockStep> seen = [];
        map.StepDetected += (_, e) => seen.Add(e.Step);

        source.Timestamp += 1_000_000_000;
        source.UtcTicks += TimeSpan.TicksPerSecond;
        Assert.IsNull(map.Sample());
        Assert.IsNull(map.LastStep);

        source.Timestamp += 1_000_000_000;
        source.UtcTicks += TimeSpan.TicksPerSecond + TimeSpan.FromSeconds(3).Ticks; // wall clock jumps 3 s forward
        ClockStep? step = map.Sample();
        Assert.IsNotNull(step);
        Assert.AreEqual(TimeSpan.FromSeconds(3), step.Size);
        Assert.AreEqual(new DateTimeOffset(source.UtcTicks, TimeSpan.Zero), step.At);
        Assert.AreEqual(step, map.LastStep);
        Assert.HasCount(1, seen);
        Assert.AreEqual(step, map.GetStatus().LastStep);
        Assert.AreEqual(source.UtcTicks, map.ToUtcTicks(source.Timestamp));

        // A backward step is negative.
        source.Timestamp += 1_000_000_000;
        source.UtcTicks += TimeSpan.TicksPerSecond - TimeSpan.FromMilliseconds(40).Ticks;
        Assert.AreEqual(TimeSpan.FromMilliseconds(-40), map.Sample()!.Size);
    }

    [TestMethod]
    public void Map_Slew_WithinMaxRate_IsFollowedButNotAStep()
    {
        FakeTimeSource source = NewSource();
        using AbsoluteClockMap map = new(source: source, probe: new FakeProbe(null));
        long before = map.Offset.Ticks;
        // 400 ppm slew for 1 s = 0.4 ms (under 1 ms threshold + 0.5 ms allowance).
        source.Timestamp += 1_000_000_000;
        source.UtcTicks += TimeSpan.TicksPerSecond + 4_000;
        Assert.IsNull(map.Sample());
        Assert.AreEqual(before + 4_000, map.Offset.Ticks);
        Assert.IsNull(map.LastStep);
    }

    [TestMethod]
    public void Map_ChangeJustBeyondAllowance_IsAStep()
    {
        FakeTimeSource source = NewSource();
        using AbsoluteClockMap map = new(source: source, probe: new FakeProbe(null));
        // Allowance for 1 s: 1 ms + 0.5 ms = 15,000 ticks. 15,001 is a step.
        source.Timestamp += 1_000_000_000;
        source.UtcTicks += TimeSpan.TicksPerSecond + 15_001;
        Assert.IsNotNull(map.Sample());
    }

    [TestMethod]
    public void Map_Conversion_IsThreadSafeWhileSampling()
    {
        FakeTimeSource source = NewSource();
        using AbsoluteClockMap map = new(source: source, probe: new FakeProbe(null));
        long expected = map.ToUtcTicks(source.Timestamp);
        Parallel.For(0, 2000, i =>
        {
            if (i % 50 == 0)
            {
                map.Sample();
            }
            Assert.AreEqual(expected, map.ToUtcTicks(source.Timestamp));
        });
    }

    [TestMethod]
    public void Map_Status_BeforeAndAfterProbe()
    {
        FakeTimeSource source = NewSource();
        ClockDisciplineReading reading = new(ClockQuality.Ntp, TimeSpan.FromMilliseconds(7), "ntp:example", "synthetic");
        FakeProbe probe = new(reading);
        using AbsoluteClockMap map = new(source: source, probe: probe);

        ClockStatus before = map.GetStatus();
        Assert.AreEqual(ClockQuality.System, before.Quality);
        Assert.IsNull(before.EstimatedError);
        Assert.AreEqual($"system:{MonotonicClock.MachineId}", before.AbsoluteClockId);
        Assert.AreEqual(MonotonicClock.Id, before.MonotonicClockId);
        Assert.AreEqual(0, probe.Calls);

        map.PollDiscipline();
        ClockStatus after = map.GetStatus();
        Assert.AreEqual(ClockQuality.Ntp, after.Quality);
        Assert.AreEqual(TimeSpan.FromMilliseconds(7), after.EstimatedError);
        Assert.AreEqual("ntp:example", after.AbsoluteClockId);
        Assert.AreEqual("synthetic", after.Detail);
        Assert.AreEqual(new DateTimeOffset(source.UtcTicks, TimeSpan.Zero), after.UpdatedAt);
    }

    [TestMethod]
    public void Map_ThrowingProbe_NeverThrows_AndReadsAsSystemUnknown()
    {
        using AbsoluteClockMap map = new(source: NewSource(), probe: new FakeProbe(null, new InvalidOperationException("boom")));
        ClockDisciplineReading reading = map.PollDiscipline();
        Assert.IsTrue(reading.Failed);
        Assert.AreEqual(ClockQuality.System, reading.Quality);
        Assert.Contains("boom", reading.Detail);
        Assert.AreEqual(ClockQuality.System, map.GetStatus().Quality);
    }

    [TestMethod]
    public async Task Map_Timer_SamplesAndProbesWithoutBlockingCaller()
    {
        FakeProbe probe = new(ClockDisciplineReading.FreeRunning("synthetic"));
        using AbsoluteClockMap map = new(new AbsoluteClockMapOptions { SampleInterval = TimeSpan.FromMilliseconds(5) }, NewSource(), probe);
        map.StartSampling();
        map.StartSampling();
        for (int i = 0; i < 400 && probe.Calls == 0; i++)
        {
            await Task.Delay(5, TestContext.CancellationToken);
        }
        Assert.IsGreaterThan(0, probe.Calls);
        Assert.IsNull(map.LastTimerError);
    }

    // ---------------------------------------------------------------- Windows Time service interpretation

    private const string UnsynchronizedVerbatim = """
        Leap Indicator: 3(not synchronized)
        Stratum: 0 (unspecified)
        Precision: -23 (119.209ns per tick)
        Root Delay: 0.0000000s
        Root Dispersion: 0.0000000s
        ReferenceId: 0x00000000 (unspecified)
        Last Successful Sync Time: unspecified
        Source: Local CMOS Clock
        Poll Interval: 10 (1024s)

        Phase Offset: 0.0000000s
        ClockRate: 0.0156250s
        State Machine: 0 (Unset)
        Time Source Flags: 0 (None)
        Server Role: 0 (None)
        Last Sync Error: 1 (The computer did not resync because no time data was available.)
        Time since Last Good Sync Time: 9043.7s
        """;

    // SYNTHETIC: laid out like the real output above, with values invented for a synchronized machine.
    private const string SyntheticNtp = """
        Leap Indicator: 0(no warning)
        Stratum: 3 (secondary reference - syncd by (S)NTP)
        Precision: -23 (119.209ns per tick)
        Root Delay: 0.0400000s
        Root Dispersion: 0.0100000s
        ReferenceId: 0x0A000001 (source IP:  10.0.0.1)
        Last Successful Sync Time: 11/16/2026 9:00:00 AM
        Source: Time.Example.com,0x8
        Poll Interval: 10 (1024s)

        Phase Offset: -0.0050000s
        ClockRate: 0.0156250s
        State Machine: 2 (Sync)
        Time Source Flags: 1 (StartedFromPeer)
        Server Role: 0 (None)
        Last Sync Error: 0 (The command completed successfully.)
        Time since Last Good Sync Time: 120.5s
        """;

    [TestMethod]
    public void Windows_Unsynchronized_Verbatim_IsFreeRunningSystem()
    {
        ClockDisciplineReading r = WindowsTimeServiceProbe.Parse(UnsynchronizedVerbatim);
        Assert.AreEqual(ClockQuality.System, r.Quality);
        Assert.IsNull(r.EstimatedError);
        Assert.IsFalse(r.Failed);
        Assert.AreEqual($"system:{MonotonicClock.MachineId}", r.AbsoluteClockId);
        Assert.AreEqual("w32time reports not synchronized (source Local CMOS Clock)", r.Detail);
    }

    [TestMethod]
    public void Windows_Unsynchronized_WithCrLf_Parses()
    {
        Assert.AreEqual(ClockQuality.System, WindowsTimeServiceProbe.Parse(UnsynchronizedVerbatim.ReplaceLineEndings("\r\n")).Quality);
    }

    [TestMethod]
    public void Windows_SyntheticNtp_IsNtpWithBoundAndNormalizedId()
    {
        ClockDisciplineReading r = WindowsTimeServiceProbe.Parse(SyntheticNtp);
        Assert.AreEqual(ClockQuality.Ntp, r.Quality);
        Assert.AreEqual("ntp:time.example.com", r.AbsoluteClockId);
        // root dispersion 10 ms + root delay 40 ms / 2 + |phase offset| 5 ms
        Assert.AreEqual(TimeSpan.FromMilliseconds(35), r.EstimatedError);
        Assert.IsFalse(r.Failed);
    }

    [TestMethod]
    public void Windows_SyntheticNtp_TwoMachinesSameServer_GetTheSameId_AndAreComparable()
    {
        string other = SyntheticNtp.Replace("Source: Time.Example.com,0x8", "Source: time.example.com.,0x9");
        ClockDisciplineReading a = WindowsTimeServiceProbe.Parse(SyntheticNtp);
        ClockDisciplineReading b = WindowsTimeServiceProbe.Parse(other);
        Assert.AreEqual(a.AbsoluteClockId, b.AbsoluteClockId);
        OrderingSeparation sep = OrderingSeparation.Minimum(
            new OrderingParticipant(TimeSpan.FromMilliseconds(1), new ClockStatus("m1", a.AbsoluteClockId, a.Quality, a.EstimatedError)),
            new OrderingParticipant(TimeSpan.FromMilliseconds(1), new ClockStatus("m2", b.AbsoluteClockId, b.Quality, b.EstimatedError)));
        Assert.AreEqual(OrderingRule.SharedTimeSource, sep.Rule);
        Assert.AreEqual(TimeSpan.FromMilliseconds(71), sep.Separation);
    }

    [TestMethod]
    public void Windows_SyntheticPtpProvider_IsPtpSoftware()
    {
        // SYNTHETIC and UNVERIFIED: the real Source text of a Windows PTP provider has not been observed.
        ClockDisciplineReading r = WindowsTimeServiceProbe.Parse(SyntheticNtp.Replace("Source: Time.Example.com,0x8", "Source: PtpProvider"));
        Assert.AreEqual(ClockQuality.PtpSoftware, r.Quality);
        Assert.AreEqual("ptp:windows-provider", r.AbsoluteClockId);
        Assert.IsNotNull(r.EstimatedError);
    }

    [TestMethod]
    public void Windows_SynchronizedToLocalClockOrHyperV_IsFreeRunning()
    {
        // SYNTHETIC
        ClockDisciplineReading local = WindowsTimeServiceProbe.Parse(SyntheticNtp.Replace("Source: Time.Example.com,0x8", "Source: Local CMOS Clock"));
        Assert.AreEqual(ClockQuality.System, local.Quality);
        Assert.IsNull(local.EstimatedError);
        ClockDisciplineReading vm = WindowsTimeServiceProbe.Parse(SyntheticNtp.Replace("Source: Time.Example.com,0x8", "Source: VMICTimeProvider"));
        Assert.AreEqual(ClockQuality.System, vm.Quality);
        Assert.IsNull(vm.EstimatedError);
        Assert.Contains("Hyper-V", vm.Detail);
    }

    [TestMethod]
    public void Windows_InvalidStratum_IsFreeRunning()
    {
        ClockDisciplineReading r = WindowsTimeServiceProbe.Parse(SyntheticNtp.Replace("Stratum: 3 (secondary", "Stratum: 0 (secondary"));
        Assert.AreEqual(ClockQuality.System, r.Quality);
        Assert.IsNull(r.EstimatedError);
    }

    [TestMethod]
    public void Windows_UnrecognizedOrMalformed_IsFailureSystemUnknown()
    {
        string[] bad =
        [
            null!,
            "",
            "   ",
            "garbage",
            "Indicateur de saut : 0(aucun avertissement)\nStrate : 3\nSource : time.example.com",       // non-English labels
            SyntheticNtp.Replace("Leap Indicator: 0", "Leap Indicator: x"),
            SyntheticNtp.Replace("Leap Indicator: 0(no warning)", "Leap Indicator: 9(?)"),
            SyntheticNtp.Replace("Root Delay: 0.0400000s", "Root Delay: 0,0400000s"),                 // localized decimal separator
            SyntheticNtp.Replace("Root Dispersion: 0.0100000s", "Root Dispersion: abc"),
            SyntheticNtp.Replace("Phase Offset: -0.0050000s", "Phase Offset: NaNs"),
            SyntheticNtp.Replace("Phase Offset: -0.0050000s", string.Empty),
            SyntheticNtp.Replace("Source: Time.Example.com,0x8", "Source:"),
            SyntheticNtp.Replace("Source: Time.Example.com,0x8", "Source: bad host!name"),
            SyntheticNtp.Replace("Root Dispersion: 0.0100000s", "Root Dispersion: 999999999s"),
            new string('x', 100_000),
        ];
        foreach (string? text in bad)
        {
            ClockDisciplineReading r = WindowsTimeServiceProbe.Parse(text);
            Assert.AreEqual(ClockQuality.System, r.Quality, text);
            Assert.IsNull(r.EstimatedError, text);
            Assert.IsTrue(r.Failed, text);
            Assert.IsFalse(string.IsNullOrEmpty(r.Detail));
        }
    }

    [TestMethod]
    public void Windows_Probe_CommandFailures_AreFailureSystemUnknown()
    {
        (Func<TimeSpan, CommandResult> run, string expected)[] cases =
        [
            (_ => new CommandResult(false, false, -1, "", "Win32Exception: not found"), "could not run w32tm"),
            (_ => new CommandResult(true, true, -1, "", null), "did not answer"),
            (_ => new CommandResult(true, false, 5, "", "Access is denied. (0x80070005)"), "Access is denied"),
            (_ => throw new IOException("pipe broke"), "pipe broke"),
            (_ => new CommandResult(true, false, 0, "unparseable", null), "not recognized"),
        ];
        foreach ((Func<TimeSpan, CommandResult> run, string expected) in cases)
        {
            ClockDisciplineReading r = new WindowsTimeServiceProbe(run, TimeSpan.FromSeconds(1)).Probe();
            Assert.AreEqual(ClockQuality.System, r.Quality);
            Assert.IsNull(r.EstimatedError);
            Assert.IsTrue(r.Failed);
            Assert.Contains(expected, r.Detail);
        }
    }

    [TestMethod]
    public void Windows_Probe_PassesVerbatimOutputThrough()
    {
        ClockDisciplineReading r = new WindowsTimeServiceProbe(_ => new CommandResult(true, false, 0, UnsynchronizedVerbatim, null), TimeSpan.FromSeconds(1)).Probe();
        Assert.AreEqual("w32time reports not synchronized (source Local CMOS Clock)", r.Detail);
    }

    [TestMethod]
    [OSCondition(OperatingSystems.Windows)]
    public void Windows_Probe_RealRun_IsTimeBounded()
    {
        // Real runner with a timeout far shorter than w32tm needs.
        Stopwatch sw = Stopwatch.StartNew();
        ClockDisciplineReading r = new WindowsTimeServiceProbe().Probe();
        Assert.IsLessThan(WindowsTimeServiceProbe.s_defaultTimeout.TotalSeconds + 5, sw.Elapsed.TotalSeconds);
        Assert.IsNotNull(r);
    }

    [TestMethod]
    public void SystemProbe_RealRun_NeverThrows_AndReportsSomething()
    {
        ClockDisciplineReading r = SystemClockDisciplineProbe.Instance.Probe();
        Console.WriteLine($"Real probe: quality={r.Quality} error={r.EstimatedError} id={r.AbsoluteClockId} failed={r.Failed} detail={r.Detail}");
        Assert.IsFalse(string.IsNullOrEmpty(r.AbsoluteClockId));
        Assert.IsFalse(string.IsNullOrEmpty(r.Detail));
        if (r.Quality == ClockQuality.System)
        {
            Assert.IsNull(r.EstimatedError);
        }
    }

    // ---------------------------------------------------------------- Linux interpretation (pure; the native call is NOT RUN)

    [TestMethod]
    public void Linux_Synchronized_UsesMaxError()
    {
        ClockDisciplineReading r = LinuxAdjtimexProbe.Interpret(0, 0, 0x2001 /* STA_PLL | STA_NANO, no UNSYNC */, 1_234);
        Assert.AreEqual(ClockQuality.Ntp, r.Quality);
        Assert.AreEqual(TimeSpan.FromTicks(12_340), r.EstimatedError);
        Assert.AreEqual($"ntp:unidentified:{MonotonicClock.MachineId}", r.AbsoluteClockId);
    }

    [TestMethod]
    public void Linux_Unsynchronized_Variants_AreFreeRunning()
    {
        Assert.AreEqual(ClockQuality.System, LinuxAdjtimexProbe.Interpret(5, 0, 0, 100).Quality);        // TIME_ERROR
        Assert.AreEqual(ClockQuality.System, LinuxAdjtimexProbe.Interpret(0, 0, 0x40, 100).Quality);     // STA_UNSYNC
        ClockDisciplineReading atLimit = LinuxAdjtimexProbe.Interpret(0, 0, 0, 16_000_000);
        Assert.AreEqual(ClockQuality.System, atLimit.Quality);
        Assert.IsNull(atLimit.EstimatedError);
        Assert.IsFalse(atLimit.Failed);
    }

    [TestMethod]
    public void Linux_Failures_AreFailureSystemUnknown()
    {
        ClockDisciplineReading failed = LinuxAdjtimexProbe.Interpret(-1, 1, 0, 0);
        Assert.IsTrue(failed.Failed);
        Assert.Contains("errno 1", failed.Detail);
        Assert.IsTrue(LinuxAdjtimexProbe.Interpret(0, 0, 0, -5).Failed);
        Assert.IsTrue(new LinuxAdjtimexProbe(() => throw new DllNotFoundException("libc")).Probe().Failed);
    }

    [TestMethod]
    public void Linux_Probe_UsesInjectedRead()
    {
        LinuxTimex timex = new() { Status = 0, MaxError = 500 };
        ClockDisciplineReading r = new LinuxAdjtimexProbe(() => (0, 0, timex)).Probe();
        Assert.AreEqual(ClockQuality.Ntp, r.Quality);
        Assert.AreEqual(TimeSpan.FromTicks(5_000), r.EstimatedError);
    }

    [TestMethod]
    public void Linux_TimexStruct_HasTheKernelSize()
    {
        // struct timex is 208 bytes on LP64 Linux (x86_64, aarch64).
        Assert.AreEqual(208, System.Runtime.InteropServices.Marshal.SizeOf<LinuxTimex>());
    }

    // ---------------------------------------------------------------- Serialization

    private static ClockStatus Full() => new(
        "win:abc:boot1", "ntp:time.example.com", ClockQuality.Ntp, TimeSpan.FromTicks(123_456),
        new ClockStep(new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero), TimeSpan.FromMilliseconds(-40)),
        new DateTimeOffset(2026, 11, 16, 12, 5, 0, TimeSpan.Zero), "detail");

    [TestMethod]
    public void MessagePack_RoundTrips_FullAndMinimal()
    {
        foreach (ClockStatus value in new[] { Full(), new ClockStatus("m", "a", ClockQuality.System, null) })
        {
            ClockStatus? back = MessagePackSerializer.Deserialize<ClockStatus>(MessagePackSerializer.Serialize(value, cancellationToken: TestContext.CancellationToken), cancellationToken: TestContext.CancellationToken);
            Assert.AreEqual(value, back);
        }
        Assert.IsNull(MessagePackSerializer.Deserialize<ClockStatus?>(MessagePackSerializer.Serialize<ClockStatus?>(null, cancellationToken: TestContext.CancellationToken), cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public void MessagePack_OlderWriter_WithFewerFields_TakesDefaults()
    {
        // A writer that only knew the first 5 fields, and one that knew 6 (no UpdatedAt, no Detail).
        foreach (int fields in new[] { 5, 6, 7 })
        {
            ClockStatus full = Full();
            byte[] bytes = Handwrite(full, fields, extra: 0);
            ClockStatus? back = MessagePackSerializer.Deserialize<ClockStatus>(bytes, cancellationToken: TestContext.CancellationToken);
            Assert.IsNotNull(back);
            Assert.AreEqual(full.AbsoluteClockId, back.AbsoluteClockId);
            Assert.AreEqual(full.EstimatedError, back.EstimatedError);
            Assert.AreEqual(fields > 5 ? full.LastStep : null, back.LastStep);
            Assert.AreEqual(fields > 6 ? full.UpdatedAt : null, back.UpdatedAt);
            Assert.IsNull(back.Detail);
        }
    }

    [TestMethod]
    public void MessagePack_NewerWriter_WithExtraFields_IsSkipped()
    {
        ClockStatus full = Full();
        ClockStatus? back = MessagePackSerializer.Deserialize<ClockStatus>(Handwrite(full, 8, extra: 3), cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(full, back);
    }

    [TestMethod]
    public void MessagePack_TooFewFields_Throws()
    {
        Assert.Throws<MessagePackSerializationException>(() => MessagePackSerializer.Deserialize<ClockStatus>(Handwrite(Full(), 4, 0), cancellationToken: TestContext.CancellationToken));
    }

    [TestMethod]
    public void MessagePack_UnknownQualityValue_IsPreserved()
    {
        ClockStatus value = new("m", "a", (ClockQuality)42, null);
        Assert.AreEqual((ClockQuality)42, MessagePackSerializer.Deserialize<ClockStatus>(MessagePackSerializer.Serialize(value, cancellationToken: TestContext.CancellationToken), cancellationToken: TestContext.CancellationToken)!.Quality);
    }

    private static byte[] Handwrite(ClockStatus s, int fieldCount, int extra)
    {
        ArrayBufferWriterHolder holder = new();
        MessagePackWriter w = new(holder.Writer);
        w.WriteArrayHeader(fieldCount + extra);
        for (int i = 0; i < fieldCount + extra; i++)
        {
            switch (i)
            {
                case 0: w.Write((byte)1); break;
                case 1: w.Write(s.MonotonicClockId); break;
                case 2: w.Write(s.AbsoluteClockId); break;
                case 3: w.Write((int)s.Quality); break;
                case 4: w.Write(s.EstimatedError!.Value.Ticks); break;
                case 5: w.WriteArrayHeader(2); w.Write(s.LastStep!.At.UtcTicks); w.Write(s.LastStep.Size.Ticks); break;
                case 6: w.Write(s.UpdatedAt!.Value.UtcTicks); break;
                case 7: w.Write(s.Detail); break;
                default: w.Write("future field"); break;
            }
        }
        w.Flush();
        return holder.ToArray();
    }

    private sealed class ArrayBufferWriterHolder
    {
        private readonly System.Buffers.ArrayBufferWriter<byte> _writer = new();
        public System.Buffers.ArrayBufferWriter<byte> Writer => _writer;
        public byte[] ToArray() => _writer.WrittenSpan.ToArray();
    }

    [TestMethod]
    public void Json_RoundTrips_WithPlainAndRepositoryOptions()
    {
        foreach (JsonSerializerOptions? options in new JsonSerializerOptions?[] { null, SystemTextJsonUtils.DefaultJsonSerializerOptions })
        {
            ClockStatus value = Full();
            string json = JsonSerializer.Serialize(value, options);
            Assert.AreEqual(value, JsonSerializer.Deserialize<ClockStatus>(json, options), json);
        }
    }

    [TestMethod]
    public void Json_OlderWriter_MissingOptionalMembers_AndUnknownMembersIgnored()
    {
        const string older = """{"monotonicClockId":"m","absoluteClockId":"a","quality":2,"estimatedError":"00:00:00.0050000"}""";
        foreach (JsonSerializerOptions? options in new JsonSerializerOptions?[] { new() { PropertyNameCaseInsensitive = true }, SystemTextJsonUtils.DefaultJsonSerializerOptions })
        {
            ClockStatus? back = JsonSerializer.Deserialize<ClockStatus>(older, options);
            Assert.IsNotNull(back);
            Assert.AreEqual(ClockQuality.Ntp, back.Quality);
            Assert.AreEqual(TimeSpan.FromMilliseconds(5), back.EstimatedError);
            Assert.IsNull(back.LastStep);
            Assert.IsNull(back.UpdatedAt);
            Assert.IsNull(back.Detail);
        }
        const string newer = """{"monotonicClockId":"m","absoluteClockId":"a","quality":1,"estimatedError":null,"somethingNew":{"x":1},"detail":"d"}""";
        ClockStatus? withExtra = JsonSerializer.Deserialize<ClockStatus>(newer, s_caseInsensitiveOptions);
        Assert.AreEqual("d", withExtra!.Detail);
    }

    public TestContext TestContext { get; set; }
}
