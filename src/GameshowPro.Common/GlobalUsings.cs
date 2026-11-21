#if WPF
global using System.Drawing;
global using System.Runtime.InteropServices.Marshalling;
global using System.Security.Principal;
global using System.ServiceProcess;
global using System.Windows;
global using System.Windows.Controls;
global using System.Windows.Controls.Primitives;
global using System.Windows.Data;
global using System.Windows.Documents;
global using System.Windows.Input;
global using System.Windows.Markup;
global using System.Windows.Media;
global using System.Windows.Threading;
global using Microsoft.WindowsAPICodePack.Dialogs;
global using Microsoft.Xaml.Behaviors;
#endif

global using System;
global using System.Buffers;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Collections.Frozen;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Collections.ObjectModel;
global using System.Collections.Specialized;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Net;
global using System.Net.NetworkInformation;
global using System.Reflection;
global using System.Runtime.InteropServices;
global using System.Runtime.Serialization;
global using System.Runtime.Versioning;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Text.Json.Serialization.Metadata;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Win32;
global using MessagePack;
global using MessagePack.Formatters;
global using GameshowPro.Common;
global using GameshowPro.Common.JsonConverters;
global using GameshowPro.Common.MessagePack;
global using GameshowPro.Common.Model;
global using GameshowPro.Common.ViewModel;
global using static GameshowPro.Common.Utils;
#if WPF
global using static GameshowPro.Common.NativeMethods;
global using static GameshowPro.Common.Wpf.Utils;
#endif

global using ILogger = Microsoft.Extensions.Logging.ILogger;
#if WPF
global using Color = System.Windows.Media.Color;
global using Point = System.Windows.Point;
global using Rect = GameshowPro.Common.NativeMethods.Rect;
global using Size = System.Windows.Size;
#endif

