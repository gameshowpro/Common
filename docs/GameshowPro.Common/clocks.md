# Clocks

Cross-platform (all four build configurations), in `GameshowPro.Common.Model`. Source: `src/GameshowPro.Common/Model/Clocks/`.

## Two clocks, two identities

| Clock | Type | What it is | Identity |
|---|---|---|---|
| Monotonic | `MonotonicClock` | `Stopwatch` timestamps as a `TimeSpan` on a fixed origin. Never steps. Stamp events with it in the capture path. | `MonotonicClock.Id`: this machine and this boot |
| Absolute | `AbsoluteClockMap` | The wall clock (UTC), obtained from a monotonic timestamp with a sampled offset, so a step in the wall clock cannot reorder edges. | `ClockStatus.AbsoluteClockId`: the time source that disciplines the wall clock |

`ClockStatus` carries both identities plus `Quality`, `EstimatedError`, `LastStep`, `UpdatedAt` and `Detail`, and is what a server reports so a client can decide whether two stamps are comparable (`OrderingSeparation.Minimum`).

### The monotonic origin

`MonotonicClock.FromTimestamp` maps Stopwatch timestamp 0 to `TimeSpan.Zero` (100 ns resolution). Two processes on one boot read comparable values:

- Windows: `Stopwatch` is QueryPerformanceCounter. Microsoft Learn ("Acquiring high-resolution time stamps") says QPC results are consistent across processors, threads and processes, that it is independent of the system time, and that it returns "the total number of ticks that have occurred since the Windows operating system was started, including the time when the machine was in a sleep state". Checked on this repository's workstation: separate processes agree with `Environment.TickCount64` to within its resolution.
- Linux: `Stopwatch` is `clock_gettime(CLOCK_MONOTONIC)` in nanoseconds (dotnet/runtime `src/native/minipal/time.c`; `SystemNative_GetTimestamp` calls it). clock_gettime(2): "a nonsettable system-wide clock", zero is boot, does not count suspend. Not run on Linux in this pass. (Time namespaces can offset it per container; not handled.)

Different boots or machines have unrelated origins, hence the identity.

### Monotonic identity

- Windows: `win:{MachineGuid}:boot{BootId}`. MachineGuid is `HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid`; BootId is the DWORD `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters\BootId`, which increments each boot. Both are readable by any user. BootId is undocumented; if prefetching were disabled and it stopped incrementing, two boots of one machine would share an id (machines still never collide). Rejected: boot time from `Environment.TickCount64` or NtQuerySystemInformation (jitter, and the boot time moves when the wall clock is stepped, which would give processes on one boot different ids).
- Linux: `linux:{machine-id}:{boot_id}` from `/etc/machine-id` (or `/var/lib/dbus/machine-id`) and `/proc/sys/kernel/random/boot_id`, both world-readable. Without a machine-id: `linux:boot:{boot_id}` (boot_id alone is unique per boot).
- Fallback, when anything needed cannot be read (or on other operating systems): `proc-random:{guid}`, a random value fixed for the process. It cannot collide with another machine or process, and nothing else can share it; `IsIdVerified` is false. `MachineId` has the same rules.

## `AbsoluteClockMap`

Offset = wall clock minus monotonic time. Sampled with bracketed reads (monotonic, wall, monotonic; wall taken at the midpoint, uncertainty half the bracket), best of 8. Defaults and reasons (`AbsoluteClockMapOptions`):

- Sample every 1 s on a timer (`StartSampling`) or on demand (`Sample`). A slewing agent changes the offset slowly (at most hundreds of microseconds per second), a step is caught within a second, a sample costs about a microsecond.
- Step threshold 1 ms plus 500 ppm of the time since the previous sample plus both samples' uncertainty. 500 ppm is the Linux kernel's and ntpd's slew limit, crystal drift is tens of ppm; below 1 ms a "step" is indistinguishable from scheduling noise. Agents that slew faster (chrony can, Windows can by percent) have their fastest corrections reported as steps: the conservative direction. The offset in force is replaced by every sample either way, so slewing is followed.
- A step raises `StepDetected` (on the sampling thread, outside the lock) and is kept as `LastStep`.
- Conversion (`ToUtc`, `ToUtcTicks`, `ToTimestamp`) is one volatile read and integer arithmetic: no lock, no allocation, no clock reads. A timestamp captured before a step but converted after the next sample uses the new offset; no offset history is kept.
- Both clocks are injectable (`ITimeSource`) so tests drive steps without sleeping.

## Discipline probing

`IClockDisciplineProbe` answers "is the wall clock disciplined, and how well", polled by the map's timer every 60 s (or `PollDiscipline`), never on the conversion path. Every failure (error, timeout, access denied, unparseable answer) gives `Quality = System`, unknown error, `Failed = true` and the reason in `Detail`; probes never throw. `ClockQuality.None` is never produced by the built-in probes.

- **Windows** (`WindowsTimeServiceProbe`): runs `w32tm /query /status /verbose` (5 s timeout, killed on timeout) and parses it. Chosen because it is the only source that gives synchronization state. Rejected: the "Windows Time Service" performance counters (via PDH English names) give offset and delay but no state, and read 0 when the clock is not synchronized, indistinguishable from "perfectly synchronized"; registry (`W32Time\Parameters\Type`, `TimeProviders\*\Enabled`) says only what is configured; `w32tm /query /source` and `/configuration` are denied without elevation. Only recognized English labels are accepted; on non-English Windows the output is not recognized and the reading is a failure (not verified). Numeric fields are used where they exist (leap indicator, stratum, root delay, root dispersion, phase offset).
  - Leap indicator 3: not synchronized: `System`, unknown error, id `system:{machine id}`, detail `w32time reports not synchronized (source ...)`.
  - Synchronized to the local clock or the Hyper-V host (`VMICTimeProvider`): `System` (accuracy not reported).
  - Synchronized to a server: `Ntp`, id `ntp:{source}` where source is the Source field with provider flags (`,0x8`) and anything after `|` removed, trailing dot removed, lower-cased. Machines following the same configured name share an id. A pool name that resolves to different servers on different machines still shares an id; this is accepted because each machine's error bound is against the root of the NTP tree, but a private pool of servers that disagree with each other would be wrongly treated as one source. Different server names never match (conservative).
  - `PtpSoftware` only if the Source is exactly `PtpProvider` (an unverified guess at the Windows PTP provider's text; never observed).
  - Error: root dispersion + root delay / 2 (the NTP root synchronization distance) + |phase offset| (so a clock still being slewed in is not reported as better than it is). This is the time service's own estimate, not a measurement.
- **Linux, 64-bit** (`LinuxAdjtimexProbe`): `adjtimex(2)` with modes = 0 (read-only, unprivileged) through source-generated P/Invoke. **Not run on Linux**: only the interpretation (`Interpret`) is unit-tested. State `TIME_ERROR`, `STA_UNSYNC`, or `maxerror` at the kernel's 16 s limit: `System`, unknown error. Otherwise `Ntp` with `EstimatedError = maxerror`: the kernel's maintained upper bound (ntpd and chrony set it from the root distance; the kernel grows it by 500 ppm while no update arrives, so a dead agent shows a growing bound). `esterror` is a statistical estimate that can be optimistic, so it is not used. adjtimex cannot name the source (and a PTP agent via phc2sys ends up here too), so the id is `ntp:unidentified:{machine id}`: never shared with another machine until a source can be named. I looked at: the timex fields (no source), `/sys/class/ptp` (only says a PTP hardware clock exists, not that the system clock follows it) and did not shell out to `pmc` or `chronyc`. Docker's default seccomp profile blocks adjtimex: that reads as a failure.
- Third-party agents (Domain Time II, Dante's PTP client, ptp4l with phc2sys) are not detected: such a machine reads as `System` with unknown error unless the OS itself reports it synchronized.

## `OrderingSeparation.Minimum(a, b)`

A pure function over `(resolution, ClockStatus)` pairs. An unknown (or negative) resolution on either side is not comparable. Same `MonotonicClockId`: `max(resolutions)`, rule `SameMonotonicClock`. Otherwise the same `AbsoluteClockId`, both qualities above `None`, both errors known: `max(resolutions) + errorA + errorB`, rule `SharedTimeSource`. Otherwise `NotComparable` with a reason. A `System` clock with unknown error is therefore never comparable across machines.

## Serialization of `ClockStatus`

Append-only, like the other report types. MessagePack: an array `[format version, MonotonicClockId, AbsoluteClockId, Quality, EstimatedError ticks, LastStep, UpdatedAt UTC ticks, Detail]`, `LastStep` being `[At UTC ticks, Size ticks]` or nil. A reader needs at least 5 elements, defaults the optional later ones and skips extra ones. System.Text.Json: camelCase properties, missing optional properties default, unknown properties are ignored (members carry `[DataMember]` so the repository's default options include them). Enum numbers are preserved if unknown; string-based JSON readers do not tolerate a quality name they do not know.
