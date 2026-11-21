namespace GameshowPro.Common.Blazor.Test;

/// <summary>
/// The CSS carries literal pre-start defaults for the tokens (the splash screen shows before MudBlazor has written its
/// variables). They must equal the C# palettes, and every MudBlazor variable the CSS reads must actually be emitted.
/// </summary>
[TestClass]
public partial class HouseThemeCssTests
{
    [GeneratedRegex(@"--gsp-default-([a-z0-9-]+):\s*([^;]+);")]
    private static partial Regex DefaultTokenRegex();

    [GeneratedRegex(@"var\((--mud-palette-[a-z-]+)")]
    private static partial Regex MudPaletteVarRegex();

    private static string CssPath()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "src", "GameshowPro.Common.Blazor", "wwwroot", "gsp-house-base.css");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }
        throw new FileNotFoundException("gsp-house-base.css not found above " + AppContext.BaseDirectory);
    }

    private static (Dictionary<string, string> Light, Dictionary<string, string> Dark) ReadDefaults()
    {
        string css = File.ReadAllText(CssPath());
        int dark = css.LastIndexOf("@media (prefers-color-scheme: dark)", StringComparison.Ordinal);
        Assert.IsGreaterThan(0, dark, "dark block not found");
        static Dictionary<string, string> Read(string text) => DefaultTokenRegex().Matches(text)
            .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value.Trim());
        return (Read(css[..dark]), Read(css[dark..]));
    }

    private static void AssertDefaults(Dictionary<string, string> actual, Palette p, GspAccentColors accent, GspAccentColors secondary, string shadow)
    {
        Dictionary<string, string> expected = new()
        {
            ["bg"] = GspContrast.Hex(p.Background), ["panel"] = GspContrast.Hex(p.Surface), ["sunken"] = GspContrast.Hex(p.BackgroundGray),
            ["ink"] = GspContrast.Hex(p.TextPrimary), ["muted"] = GspContrast.Hex(p.TextSecondary), ["line"] = GspContrast.Hex(p.LinesDefault),
            ["ok"] = GspContrast.Hex(p.Success), ["warn"] = GspContrast.Hex(p.Warning), ["bad"] = GspContrast.Hex(p.Error),
            ["accent"] = accent.Fill, ["accent-ink"] = accent.OnFill, ["accent-text"] = accent.Text,
            ["accent-2"] = secondary.Fill, ["accent-2-ink"] = secondary.OnFill, ["accent-2-text"] = secondary.Text,
            ["shadow"] = shadow,
        };
        foreach ((string key, string value) in expected)
        {
            Assert.IsTrue(actual.TryGetValue(key, out string? found), $"--gsp-default-{key} missing");
            Assert.AreEqual(value, found, $"--gsp-default-{key}");
        }
    }

    [TestMethod]
    public void Css_defaults_equal_the_indigo_palettes()
    {
        (Dictionary<string, string> light, Dictionary<string, string> dark) = ReadDefaults();
        MudTheme theme = GspHouseTheme.Create(GspAccent.Indigo);
        AssertDefaults(light, theme.PaletteLight, GspAccent.Indigo.Light, GspHouseTheme.NeutralSecondaryLight, "0 1px 3px rgba(0, 0, 0, .07)");
        AssertDefaults(dark, theme.PaletteDark, GspAccent.Indigo.Dark, GspHouseTheme.NeutralSecondaryDark, "none");
    }

    [TestMethod]
    public void Css_tokens_read_mudblazor_variables_that_the_theme_provider_emits()
    {
        string css = File.ReadAllText(CssPath());
        string[] read = [.. MudPaletteVarRegex().Matches(css).Select(m => m.Groups[1].Value).Distinct()];
        Assert.IsNotEmpty(read);

        using BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMudServices();
        IRenderedComponent<GspThemeProvider> cut = context.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, true));
        string markup = cut.Markup;
        foreach (string name in read)
        {
            Assert.Contains(name + ": ", markup, $"MudBlazor does not emit {name}");
        }
        // The dark palette is the active one.
        Assert.Contains("--mud-palette-background: rgba(19,21,26,1)", markup);
    }
}
