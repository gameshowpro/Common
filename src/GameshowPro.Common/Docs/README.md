# GameshowPro.Common
This library contains common classes and utilities used throughout GameShowPro libraries and applications and their dependencies. 
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
### Model
Various types are provided for abstract handling of incoming triggers and outgoing lighting. These types allow the used of a shared UI by multiple specific implementations in other projects. These support WPF data binding specifically. In future the logic could be refactored to be cross-platform and these types could be replaces with shallow WPF wrappers.
Documentation of the lighting classes is [here](Lighting.md).
