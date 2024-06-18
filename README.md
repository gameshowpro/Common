# GameshowPro Common libraries
This is a set of .NET 8.0 libraries which are used accross other GameshowPro projects and many closed-source projects.

## GameshowPro.Common
This library contains all classes and utilities which are available across platforms. Includes helper functions for `System.Text.Json` in `SystemTextJsonUtils` and `GameshowPro.Common.JsonConverters`. Equivilents for `Newtonsoft.Json` area available in a separate assembly package.
[See documentation here.](GameshowPro.Common/Docs/README.md)

## GameshowPro.Common.Windows
This library contains all classes and utilities that are dependent on WFP or Windows Forms. It references `GameshowPro.Common` and provides Window-specific implementations of some of its features.
[See documentation here.](GameshowPro.Common.Windows/Docs/README.md)

## GameshowPro.Common.JsonNet
A library to contain all functionality that requires a reference to `Newtonsoft.Json`.

## GameshowPro.Common.NLog
A library to contain all functionality that requires a reference to `NLog`.

# Publishing
Publishing is implemented in a [GitHub workflow.](.github/workflows/build.yaml) All projects are built from the root solution. The command `dotnet pack -c Release` automatically creates three packages with the requisite dependency structure.