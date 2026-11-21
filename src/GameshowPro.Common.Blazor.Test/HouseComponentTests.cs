namespace GameshowPro.Common.Blazor.Test;

/// <summary>The house theme provider, header and footer render what the CSS expects.</summary>
[TestClass]
public class HouseComponentTests
{
    private static BunitContext CreateContext()
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMudServices();
        return context;
    }

    [TestMethod]
    public void ThemeProvider_writes_the_accent_for_the_forced_mode()
    {
        using BunitContext context = CreateContext();
        GspAccent yellow = HouseThemeContrastTests.s_logoYellow;

        string light = context.Render<GspThemeProvider>(p => p.Add(x => x.Accent, yellow).Add(x => x.ForceDarkMode, false)).Markup;
        Assert.Contains("--gsp-accent:#ffe600;--gsp-accent-ink:#16191d;", light);
        Assert.Contains("color-scheme:light", light);
        Assert.Contains("--mud-palette-primary: rgba(123,111,0,1)", light);

        string dark = context.Render<GspThemeProvider>(p => p.Add(x => x.Accent, yellow).Add(x => x.ForceDarkMode, true)).Markup;
        Assert.Contains("--gsp-accent:#ffe600;--gsp-accent-ink:#13151a;", dark);
        Assert.Contains("--gsp-shadow:none", dark);
        Assert.Contains("--mud-palette-primary: rgba(255,230,0,1)", dark);
    }

    [TestMethod]
    public void ThemeProvider_without_a_forced_mode_renders_and_starts_light_when_the_browser_reports_nothing()
    {
        using BunitContext context = CreateContext();
        string markup = context.Render<GspThemeProvider>().Markup;
        Assert.Contains("--gsp-accent:#3b5bdb;", markup);
    }

    [TestMethod]
    public void Accent_rejects_values_that_are_not_hex_colors()
    {
        Assert.Throws<ArgumentException>(() => new GspAccentColors("red", "#ffffff", "#000000"));
        Assert.Throws<ArgumentException>(() => new GspAccentColors("#fff;}body{display:none", "#ffffff", "#000000"));
    }

    [TestMethod]
    public void AppHeader_renders_brand_version_and_actions()
    {
        using BunitContext context = CreateContext();
        IRenderedComponent<GspAppHeader> cut = context.Render<GspAppHeader>(p => p
            .Add(x => x.IconUrl, "images/icon.svg")
            .Add(x => x.ProductName, "TriggerGate")
            .Add(x => x.Version, "1.2.3")
            .Add(x => x.VersionTooltip, "Built 2026-11-19")
            .AddChildContent("<span id=\"right\">actions</span>"));

        Assert.AreEqual("TriggerGate", cut.Find("h1.gsp-app-name").TextContent);
        Assert.AreEqual("images/icon.svg", cut.Find("img.gsp-app-icon").GetAttribute("src"));
        Assert.AreEqual("1.2.3", cut.Find(".gsp-pill.muted.gsp-data").TextContent);
        Assert.AreEqual("Built 2026-11-19", cut.Find(".gsp-pill").GetAttribute("title"));
        Assert.AreEqual("actions", cut.Find(".gsp-header-actions #right").TextContent);
    }

    [TestMethod]
    public void AppHeader_omits_the_parts_it_was_not_given()
    {
        using BunitContext context = CreateContext();
        IRenderedComponent<GspAppHeader> cut = context.Render<GspAppHeader>(p => p.Add(x => x.ProductName, "TriggerGate"));
        Assert.IsEmpty(cut.FindAll("img"));
        Assert.IsEmpty(cut.FindAll(".gsp-pill"));
        Assert.IsEmpty(cut.FindAll(".gsp-header-actions"));
    }

    [TestMethod]
    public void PageFooter_renders_product_version_and_copyright()
    {
        using BunitContext context = CreateContext();
        IRenderedComponent<GspPageFooter> cut = context.Render<GspPageFooter>(p => p
            .Add(x => x.ProductName, "TriggerGate").Add(x => x.Version, "1.2.3"));
        Assert.AreEqual("TriggerGate 1.2.3, © 2026 Barjonas LLC", cut.Find("footer.gsp-footer span").TextContent);
    }
}
