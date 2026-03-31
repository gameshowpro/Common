# GameshowPro.Common

Cross-platform shared library for common utilities, models, converters, and serialization helpers.

`GameshowPro.Common.Windows` is a special build and package that targets the `-windows` TPM of .NET and includes WPF and Windows-specific features.

# System.Text.Json
`GameshowPro.Common` provides an implementation of (de)serialization around `System.Text.Json` adding features such as support for enhanced `JsonConstructor` behavior and `object` types serialized with type information.

[Details](system-text-json.md).

# Lighting
An implementation-independent object model for defining lighing fixtures, preset states, and applying presets to fixtures.

[Details](Lighting.md).

# Triggers
Classes and interfaces to provide a unified interface to triggering systems.

# ObservableUiProxy
A lightweight proxy that re-raises `INotifyPropertyChanged` events on the correct UI thread, enabling singleton models to be safely observed from Blazor Server circuits, WPF windows, or any other UI context.

[Details](ui-proxy.md).

## BaseConverters
A collection of converters which are similar in purpose to `System.Windows.Data.IValueConverter` but not dependent on WPF. These can easily be subclassed by platform-specific implementations, e.g. WPF, MAUI.
## Package references
### MessagePack
Allows common types to define custom MessagePack serialization.
### Microsoft.Extensions.Logging
Allows logging to be injected into some functions.

## Windows-specific features
When built with Windows build targets, the following features are added:

### Converters
WPF converters and markup extensions. Ultimately, all of these will be shallow wrappers for the base converters mentioned above.
### View
WPF data templates for many of the types in this library.
### Wpf
AppBase is a standardized way of instantiation a WPF application, providing some additional shrink-wrapped functionality.
WpfUtils contains an extra set of extension methods and utilities specifically targeting WPF.
### Lighting
Lighting functionality usising `Windows.Media.Color` to make WPF integration easier.

# Testing
The `GameshowPro.Common.Test` project tests various features of this library, particularly the custom (de)serialization and utilities.

[Details](testing.md).
