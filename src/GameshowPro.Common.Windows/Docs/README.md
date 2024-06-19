# GameshowPro.Common.Windows
This library contains all classes and utilities that are dependent on WFP or Windows Forms. It references `GameshowPro.Common` and provides Window-specific implementations of some of its features.
### Converters
WPF converters and markup extensions. Ultimately, all of these will be shallow wrappers for the base converters in [GameshowPro.Common](../../GameshowPro.Common/Docs/README.md).
### View
WPF data templates for many of the types in this library.
### Wpf
AppBase is a standardized way of instantiation a WPF application, providing some additional shrink-wrapped functionality.
WpfUtils contains an extra set of extension methods and utilities specicically targetting WPF.
### Model
Various types are provided for abstract handling of incoming triggers and outgoing lighting. These types allow the used of a shared UI by multiple specific implementations in other projects. These support WPF data binding specifically. In future the logic could be refactored to be cross-platform and these types could be replaces with shallow WPF wrappers.
Documentation of the lighting classes is [here](Lighting.md).