# GameshowPro Common libraries
This is a set of .NET 8.0/9.0 libraries which are used accross other GameshowPro projects and many closed-source projects.

## Builds
The default builds and packages do not include Windows-specific parts of the framework.
There are special Windows builds and packages that include references to WPF and Windows Forms.

## GameshowPro.Common
This library contains all classes and utilities which are available across platforms. Includes helper functions for `System.Text.Json` in `SystemTextJsonUtils` and `GameshowPro.Common.JsonConverters`. Equivilents for `Newtonsoft.Json` area available in a separate assembly package.
[See documentation here.](GameshowPro.Common/Docs/README.md)

## GameshowPro.Common.JsonNet
A library to contain all functionality that requires a reference to `Newtonsoft.Json`.

## GameshowPro.Common.NLog
A library to contain all functionality that requires a reference to `NLog`.

# Publishing
Publishing is implemented in a [GitHub workflow.](.github/workflows/build.yaml) All projects are built from the root solution. The command `dotnet pack -c Release` automatically creates three cross platform packages with the requisite dependency structure. `dotnet pack -c ReleaseWindows` builds the chain of packages that reference Windows frameworks.