using System.Reflection;

namespace GameshowPro.Common.Blazor;

/// <summary>
/// The app's identity as the house style shows it: what was derived from the app's assembly, with the overrides of
/// <see cref="GspHouseOptions"/> applied. Injected by the house components; build it yourself with <see cref="Resolve"/>.
/// </summary>
/// <remarks>
/// Derived from the entry assembly: <c>AssemblyProduct</c> (name), <c>AssemblyInformationalVersion</c> (the full version
/// as nbgv writes it, and the public form without the <c>+commit</c> suffix), <c>AssemblyCopyright</c>, and
/// <c>AssemblyMetadata</c> entries <c>RepositoryUrl</c> (emitted by the SDK when the project sets
/// <c>RepositoryUrl</c>), <c>GspBuildDate</c>, <c>PackageLicenseExpression</c>, <c>PackageProjectUrl</c> and
/// <c>GspDocumentationUrl</c> (the last four added by this package's <c>buildTransitive</c> targets file).
/// </remarks>
public sealed class GspAppInfo
{
    /// <summary>Copyright line used when neither the options nor the assembly give one.</summary>
    public const string DefaultCopyright = "© 2026 Barjonas LLC";

    /// <summary>The product name.</summary>
    public string ProductName { get; init; } = "";

    /// <summary>The public version, without any <c>+commit</c> suffix, for the header and the footer. Empty when unknown.</summary>
    public string Version { get; init; } = "";

    /// <summary>The full informational version including the commit suffix, for the About table. Empty when unknown.</summary>
    public string FullVersion { get; init; } = "";

    /// <summary>The build date (<c>yyyy-MM-dd</c>) when the build recorded one, else <c>null</c>.</summary>
    public string? BuildDate { get; init; }

    /// <summary>The copyright line, beginning with the copyright sign.</summary>
    public string Copyright { get; init; } = DefaultCopyright;

    /// <summary>The icon address, relative to the page's base address.</summary>
    public string IconUrl { get; init; } = "images/app-icon.svg";

    /// <summary>The tagline, or <c>null</c>.</summary>
    public string? Tagline { get; init; }

    /// <summary>The trademark notice, or <c>null</c>.</summary>
    public string? Trademark { get; init; }

    /// <summary>The license paragraph, or <c>null</c>.</summary>
    public string? LicenseText { get; init; }

    /// <summary>The SPDX license expression, or <c>null</c>.</summary>
    public string? LicenseExpression { get; init; }

    /// <summary>The About dialog's links, in order: Documentation, Source, License text, then the extra ones; only those with an address.</summary>
    public IReadOnlyList<GspAboutLink> Links { get; init; } = [];

    /// <summary>Whether the fixed About button is shown.</summary>
    public bool ShowAboutButton { get; init; } = true;

    /// <summary>
    /// Resolves the identity from <paramref name="assembly"/> (default: the entry assembly, else the calling one) and
    /// <paramref name="options"/>.
    /// </summary>
    public static GspAppInfo Resolve(GspHouseOptions options, Assembly? assembly = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        assembly ??= Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
        Dictionary<string, string> metadata = [];
        foreach (AssemblyMetadataAttribute attribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (attribute.Value is { Length: > 0 })
            {
                metadata[attribute.Key] = attribute.Value;
            }
        }

        string full = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString(3) ?? "";
        string product = options.ProductName
            ?? NullIfEmpty(assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product)
            ?? assembly.GetName().Name ?? "";
        string? copyright = options.Copyright ?? NullIfEmpty(assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright);
        string? license = metadata.GetValueOrDefault("PackageLicenseExpression");
        string? repository = options.SourceUrl ?? metadata.GetValueOrDefault("RepositoryUrl");
        string? documentation = options.DocumentationUrl ?? metadata.GetValueOrDefault("GspDocumentationUrl") ?? metadata.GetValueOrDefault("PackageProjectUrl");

        List<GspAboutLink> links = [];
        if (documentation is not null)
        {
            links.Add(new GspAboutLink("Documentation", documentation));
        }
        if (repository is not null)
        {
            links.Add(new GspAboutLink(repository.Contains("github.com", StringComparison.OrdinalIgnoreCase) ? "Source on GitHub" : "Source", repository));
        }
        if (options.LicenseUrl is not null)
        {
            links.Add(new GspAboutLink("License text", options.LicenseUrl));
        }
        links.AddRange(options.ExtraLinks);

        return new GspAppInfo
        {
            ProductName = product,
            FullVersion = full,
            Version = options.Version ?? PublicVersion(full),
            BuildDate = metadata.GetValueOrDefault("GspBuildDate"),
            Copyright = NormalizeCopyright(copyright),
            IconUrl = options.IconPath,
            Tagline = options.Tagline,
            Trademark = options.Trademark,
            LicenseExpression = license,
            LicenseText = options.LicenseText ?? (license is null ? null : $"Open source under the {license} license."),
            Links = links,
            ShowAboutButton = options.ShowAboutButton,
        };
    }

    /// <summary>The informational version without the <c>+commit</c> build metadata nbgv appends.</summary>
    public static string PublicVersion(string informationalVersion)
    {
        int plus = informationalVersion.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? informationalVersion : informationalVersion[..plus];
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string NormalizeCopyright(string? copyright)
        => copyright is null ? DefaultCopyright
            : copyright.StartsWith('©') || copyright.StartsWith("(c)", StringComparison.OrdinalIgnoreCase) ? copyright : "© " + copyright;
}
