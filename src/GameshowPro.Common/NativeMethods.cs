// (C) Barjonas LLC 2018

namespace GameshowPro.Common;

public static partial class NativeMethods
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);
    internal delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);
    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetMonitorInfo(IntPtr hMonitor, [MarshalUsing(typeof(MonitorInfoExMarshaller))] ref MonitorInfoEx lpmi);
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    // size of a device name string
    private const int CCHDEVICENAME = 32;

    internal const int MONITORINFOF_PRIMARY = 1;

    /// <summary>
        /// The MONITORINFOEX structure contains information about a display monitor.
        /// The GetMonitorInfo function stores information into a MONITORINFOEX structure or a MONITORINFO structure.
        /// The MONITORINFOEX structure is a superset of the MONITORINFO structure. The MONITORINFOEX structure adds a string member to contain a name 
        /// for the display monitor.
        /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct MonitorInfoEx
    {
        /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function. 
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
        public int Size;

        /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
        public Rect Monitor;

        /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications, 
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor. 
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars. 
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
        public Rect WorkArea;

        /// <summary>
            /// The attributes of the display monitor.
            /// 
            /// This member can be the following value:
            ///   1 : MONITORINFOF_PRIMARY
            /// </summary>
        public uint Flags;

        /// <summary>
            /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name, 
            /// and so can save some bytes by using a MONITORINFO structure.
            /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        internal string _deviceName;

        public void Init()
        {
            Size = 40 + 2 * CCHDEVICENAME;
            _deviceName = string.Empty;
        }
    }
    /// <summary>
    /// Blittable mirror of <see cref="MonitorInfoEx"/> (the string field becomes an inline UTF-16 buffer) so that
    /// <see cref="GetMonitorInfo"/> can use source-generated marshalling.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal unsafe struct MonitorInfoExUnmanaged
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
        public fixed char DeviceName[CCHDEVICENAME];
    }

    /// <summary>
    /// Converts <see cref="MonitorInfoEx"/> to and from <see cref="MonitorInfoExUnmanaged"/> for <see cref="GetMonitorInfo"/>.
    /// </summary>
    [CustomMarshaller(typeof(MonitorInfoEx), MarshalMode.ManagedToUnmanagedRef, typeof(MonitorInfoExMarshaller))]
    internal static unsafe class MonitorInfoExMarshaller
    {
        public static MonitorInfoExUnmanaged ConvertToUnmanaged(MonitorInfoEx managed)
        {
            MonitorInfoExUnmanaged unmanaged = new()
            {
                Size = managed.Size,
                Monitor = managed.Monitor,
                WorkArea = managed.WorkArea,
                Flags = managed.Flags,
            };
            ReadOnlySpan<char> name = managed._deviceName.AsSpan(0, Math.Min(managed._deviceName?.Length ?? 0, CCHDEVICENAME - 1));
            name.CopyTo(new Span<char>(unmanaged.DeviceName, CCHDEVICENAME));
            return unmanaged;
        }

        public static MonitorInfoEx ConvertToManaged(MonitorInfoExUnmanaged unmanaged)
        {
            ReadOnlySpan<char> buffer = new(unmanaged.DeviceName, CCHDEVICENAME);
            int length = buffer.IndexOf('\0');
            return new MonitorInfoEx
            {
                Size = unmanaged.Size,
                Monitor = unmanaged.Monitor,
                WorkArea = unmanaged.WorkArea,
                Flags = unmanaged.Flags,
                _deviceName = new string(length < 0 ? buffer : buffer[..length]),
            };
        }
    }

    [LibraryImport("Shcore.dll")]
    internal static partial IntPtr GetDpiForMonitor(IntPtr hmonitor, DpiType dpiType, out uint dpiX, out uint dpiY);

    /// <summary>
    /// Represents the different types of scaling.
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/dn280511.aspx">MSDN: GetDpiForMonitor</seealso>
    public enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2,
    }
}
