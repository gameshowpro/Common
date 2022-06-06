// (C) Barjonas LLC 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static Barjonas.Common.NativeMethods;

namespace Barjonas.Common
{
    public static class Screen
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfo
        {
            public int cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public uint dwFlags;
        }

        private static List<MonitorInfoEx> GetDisplays()
        {
            var col = new List<MonitorInfoEx>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
            {
                var mi = new MonitorInfoEx();
                mi.Size = Marshal.SizeOf(mi);
                bool success = GetMonitorInfo(hMonitor, ref mi);
                if (success)
                {
                    switch (GetDpiForMonitor(hMonitor, DpiType.Effective, out uint dpiX, out uint dpiY).ToInt32())
                    {
                        case S_OK:
                            Rect scaled = new()
                            {
                                left = (int)(mi.Monitor.left / ((double)dpiX / 96)),
                                right = (int)(mi.Monitor.right / ((double)dpiX / 96)),
                                top = (int)(mi.Monitor.top / ((double)dpiY / 96)),
                                bottom = (int)(mi.Monitor.bottom / ((double)dpiY / 96))
                            };
                            mi.Monitor = scaled;
                            break;
                        case E_INVALIDARG:
                            throw new ArgumentException("Unknown error. See https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510.aspx for more information.");
                        default:
                            throw new COMException("Unknown error. See https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510.aspx for more information.");
                    }
                    col.Add(mi);
                }
                return true;
            }, IntPtr.Zero);
            return col;
        }

        private const int S_OK = 0;
        private const int MONITOR_DEFAULTTONEAREST = 2;
        private const int E_INVALIDARG = -2147024809;


        /// <summary>
        /// Size a window to fill a given display.
        /// </summary>
        /// <param name="window">The Window to size.</param>
        /// <param name="index">The index of the target screen, where zero is always the primary.</param>
        public static bool SizeWindowToScreen(this System.Windows.Window window, int index)
        {
            var target = new MonitorInfoEx();
            List<MonitorInfoEx> disps = GetDisplays();
            if (index == 0)
            {
                target = disps.FirstOrDefault(d => (d.Flags & MONITORINFOF_PRIMARY) != 0);
            }
            else
            {
                int i = 0;
                foreach (MonitorInfoEx d in disps)
                {
                    if ((d.Flags & MONITORINFOF_PRIMARY) == 0)
                    {
                        i++;
                        if (i == index)
                        {
                            target = d;
                            break;
                        }
                    }
                }
            }

            Rect rect = target.Monitor;
            if (rect.left < rect.right && rect.bottom > rect.top)
            {
                SizeWindowToRect(window, rect);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SizeWindowToRect(this System.Windows.Window window, Rect target)
        {
            window.Left = target.left;
            window.Top = target.top;
            window.Width = target.right - target.left;
            window.Height = target.bottom - target.top;
        }
    }
}



