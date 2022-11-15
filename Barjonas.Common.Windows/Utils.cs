// (C) Barjonas LLC 2019
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using Barjonas.Common.Model;
using Microsoft.WindowsAPICodePack.Dialogs;

#nullable enable
namespace Barjonas.Common
{
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
                    wnd.Cursor = System.Windows.Input.Cursors.None;
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
            var target = new Rectangle();
            if (index == 0)
            {
                target = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
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
            if (target.Width > 0 && target.Height > 0)
            {
                SizeWindowToRect(window, target);
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

        public static ImmutableDictionary<TTriggerKey, IncomingTriggerComposite> ComposeDevicesIntoTriggerDictionary<TTriggerKey>(
            IEnumerable<IncomingTriggerDeviceBase<TTriggerKey>> devices)
                where TTriggerKey : struct, Enum
            => Enum.GetValues<TTriggerKey>().ToImmutableDictionary(
                    t => t, 
                    t => new IncomingTriggerComposite(devices.Select(d => d.TriggersBase[t]), t.ToString(), GetTriggerParameters(t.GetType().GetField(t.ToString()))?.Name ?? "No name")
                );

        /// <summary>
        ///  Build a multiple list of <seealso cref="IncomingTrigger"/> based on an enum type decorated with <seealso cref="TriggerParameters"/>.
        ///  Each list represents an instance of a particular trigger device.
        /// </summary>
        /// <typeparam name="TCommand">Type of command enum which is decorated with <seealso cref="TriggerParameters"/> </typeparam>
        /// <typeparam name="T">Type of trigger</typeparam>
        /// <param name="factory">A factory to create triggers.</param>
        /// <param name="dictList">A list of dictionaries each representing one instance with triggers keyed by command, which will be updated</param>
        /// <param name="settings">A list of depersisted settings, each representing one instance</param>
        /// <param name="removeUntouchedSettings"></param>
        /// <returns></returns>
        public static ImmutableList<ImmutableDictionary<TCommand, TTrigger>> BuildTriggerListPerDevice<TCommand, TTrigger>(
            Func<IncomingTriggerSetting, TTrigger> factory,
            IEnumerable<IncomingTriggerSettings> settings,
            bool removeUntouchedSettings
        )
        where TCommand : notnull, Enum
        where TTrigger : IncomingTrigger
        {
            ImmutableList<ImmutableDictionary<TCommand, TTrigger>>.Builder dictList = ImmutableList.CreateBuilder<ImmutableDictionary<TCommand, TTrigger>>();
            foreach (IncomingTriggerSettings s in settings)
            {
                _ = BuildTriggerList(factory, out ImmutableDictionary<TCommand, TTrigger> dict, s, removeUntouchedSettings);
                dictList.Add(dict);
            }
            return dictList.ToImmutable();
        }

        /// <summary>
        /// Build a list of <seealso cref="IncomingTrigger"/> based on an enum type decorated with <seealso cref="TriggerParameters"/>.
        /// </summary>
        /// <typeparam name="TCommand">Type of command enum which is decorated with <seealso cref="TriggerParameters"/> </typeparam>
        /// <typeparam name="T">Type of trigger</typeparam>
        /// <param name="factory">A factory to create triggers.</param>
        /// <param name="dict">A dictionary of triggers keyed by command, which will be updated</param>
        /// <returns></returns>
        public static List<TTrigger> BuildTriggerList<TCommand, TTrigger>(
            Func<IncomingTriggerSetting, TTrigger> factory,
            out ImmutableDictionary<TCommand, TTrigger> dict,
            IncomingTriggerSettings settings,
            bool removeUntouchedSettings
        )
        where TCommand : notnull, Enum
        where TTrigger : IncomingTrigger
        {
            var triggers = new List<TTrigger>();
            ImmutableDictionary<TCommand, TTrigger>.Builder dictBuilder = ImmutableDictionary.CreateBuilder<TCommand, TTrigger>();
            Type t = typeof(TCommand);
            foreach (TCommand value in Enum.GetValues(t))
            {
                string? valueString = value.ToString();
                if (valueString == null)
                {
                    throw new ArgumentException($"{t} cannot contain a enum value that converts to a null string.");
                }
                else
                {
                    TriggerParameters? attr = GetTriggerParameters(t.GetField(valueString));
                    if (attr is null)
                    {
                        throw new MissingMemberException($"{t} must contain a {nameof(TriggerParameters)} attribute on every member.");
                    }
                    TTrigger trigger = factory(settings.GetOrCreate(value.ToString(), attr.Name, attr.DefaultId, attr.TriggerFilter, attr.DebounceInterval));
                    triggers.Add(trigger);
                    dictBuilder.Add(value, trigger);
                }
            }
            dict = dictBuilder.ToImmutable();
            if (removeUntouchedSettings)
            {
                settings.RemoveUntouched();
            }
            return triggers;
        }

        private static TriggerParameters? GetTriggerParameters(MemberInfo? enumMemberInfo)
        {
            object[]? attrs = enumMemberInfo?.GetCustomAttributes(typeof(TriggerParameters), false);
            if (attrs?.FirstOrDefault() is not TriggerParameters attr)
            {
                return null;
            }
            return attr;
        }

        /// <summary>
        /// Build a list of <seealso cref="IncomingTrigger"/> based on a count only. No custom property values will be applied.
        /// </summary>
        /// <typeparam name="T">Type of trigger</typeparam>
        /// <param name="factory">A factory to create triggers.</param>
        /// <param name="dict">A dictionary dictionary of triggers keyed by command, which will be updated</param>
        /// <returns></returns>
        public static List<TTrigger> BuildTriggerList<TTrigger>(
            Func<IncomingTriggerSetting, TTrigger> factory,
            byte count,
            IncomingTriggerSettings settings,
            bool removeUntouchedSettings
        )
        where TTrigger : IncomingTrigger
        {
            var triggers = new List<TTrigger>();
            for (byte index = 0; index < count; index++)
            {
                TTrigger trigger = factory(settings.GetOrCreate(index.ToString(), $"Trigger {index}", index, TriggerFilter.All, TimeSpan.FromSeconds(1)));
                triggers.Add(trigger);
            }
            if (removeUntouchedSettings)
            {
                settings.RemoveUntouched();
            }
            return triggers;
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

        public static string MakePathAbsolute(this string? relativePath, Uri basePath) =>
           (relativePath == null ? basePath : new Uri(basePath, relativePath)).LocalPath;


#if !NETFRAMEWORK
        public static T[] ArrayRepeat<T>(T value, int count)
        {
            T[] array = new T[count];
            Array.Fill(array, value);
            return array;
        }
#endif
    }
}
#nullable restore
