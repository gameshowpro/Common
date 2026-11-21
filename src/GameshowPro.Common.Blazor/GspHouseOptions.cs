namespace GameshowPro.Common.Blazor;

/// <summary>A link in the About dialog, opened in a new tab.</summary>
/// <param name="Text">The link text.</param>
/// <param name="Url">The address.</param>
public sealed record GspAboutLink(string Text, string Url);

/// <summary>A label and a value in the About dialog's table of build facts.</summary>
/// <param name="Label">The row label.</param>
/// <param name="Value">The value, shown in the data face.</param>
public sealed record GspAboutRow(string Label, string Value);

/// <summary>
/// What an app tells the house style, once, in <c>AddGspHouseStyle</c>. Everything that can be derived from the app's
/// assembly (product name, version, copyright, repository URL, build date, license expression; see <see cref="GspAppInfo"/>)
/// is derived and needs no entry here; set a value only to override what was derived or to add what the build does not
/// know. Most products set <see cref="Accent"/>.
/// </summary>
public sealed class GspHouseOptions
{
    /// <summary>The product's accent (MudBlazor's <c>Primary</c>, <c>--gsp-accent*</c>, the header rule, the loading ring). Defaults to <see cref="GspAccent.Indigo"/>.</summary>
    public GspAccent Accent { get; set; } = GspAccent.Indigo;

    /// <summary>An optional second accent (MudBlazor's <c>Secondary</c>, <c>--gsp-accent-2*</c>). Without it <c>Secondary</c> is the house neutral.</summary>
    public GspAccent? SecondaryAccent { get; set; }

    /// <summary>The app icon, relative to the page's base address. The convention is <c>images/app-icon.svg</c>; the header, About dialog, connection overlay and loading screen use it.</summary>
    public string IconPath { get; set; } = "images/app-icon.svg";

    /// <summary>Overrides the product name derived from the assembly's <c>Product</c> attribute.</summary>
    public string? ProductName { get; set; }

    /// <summary>Overrides the version shown in the header and the footer (derived from the informational version without its commit suffix).</summary>
    public string? Version { get; set; }

    /// <summary>Overrides the copyright line (derived from the assembly's <c>Copyright</c> attribute, else <c>© 2026 Barjonas LLC</c>).</summary>
    public string? Copyright { get; set; }

    /// <summary>One line under the product name in the About dialog.</summary>
    public string? Tagline { get; set; }

    /// <summary>A trademark notice for the About dialog, for example <c>TriggerGate is a trademark of Barjonas LLC.</c></summary>
    public string? Trademark { get; set; }

    /// <summary>Documentation address; default the <c>GspDocumentationUrl</c> project property, else the project URL.</summary>
    public string? DocumentationUrl { get; set; }

    /// <summary>Source address; defaults to the assembly's repository URL.</summary>
    public string? SourceUrl { get; set; }

    /// <summary>Address of the license text, shown as a "License text" link.</summary>
    public string? LicenseUrl { get; set; }

    /// <summary>The short license paragraph of the About dialog; default derived from the license expression, if the build knows one.</summary>
    public string? LicenseText { get; set; }

    /// <summary>Further links after the standard ones (an EULA, sponsorship).</summary>
    public List<GspAboutLink> ExtraLinks { get; } = [];

    /// <summary>Whether <c>GspAppShell</c> shows the fixed About button at the bottom left. The About dialog itself (opened by the header's version pill and the footer) is always available.</summary>
    public bool ShowAboutButton { get; set; } = true;

    /// <summary>Whether <c>GspAppShell</c> shows the footer line (product, version, copyright). Off by default: the About dialog carries the same facts.</summary>
    public bool ShowFooter { get; set; }
}
