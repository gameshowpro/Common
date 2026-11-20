using Bunit;
using GameshowPro.Common.Model;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace GameshowPro.Common.Blazor.Test;

/// <summary>
/// The one-registration design: what <c>AddGspHouseStyle</c> derives from the app's assembly (this test assembly stands in
/// for an app; its csproj imports the package's targets file, which is how the build date and license expression arrive),
/// and the components that read the registration by injection with no parameters.
/// </summary>
[TestClass]
public class HouseStyleRegistrationTests
{
    private static BunitContext CreateContext(Action<GspHouseOptions>? configure = null)
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddGspHouseStyle(configure);
        // The tests assemble the info from this assembly; the entry assembly of a test run is the test host's.
        context.Services.AddSingleton(sp => GspAppInfo.Resolve(sp.GetRequiredService<GspHouseOptions>(), typeof(HouseStyleRegistrationTests).Assembly));
        return context;
    }

    [TestMethod]
    public void Info_is_derived_from_the_assembly_attributes_and_the_targets_file_metadata()
    {
        GspAppInfo info = GspAppInfo.Resolve(new GspHouseOptions(), typeof(HouseStyleRegistrationTests).Assembly);
        Assert.AreEqual("GameshowPro.Common.Blazor.Test", info.ProductName);
        Assert.IsFalse(info.Version.Contains('+'), "public version has no commit suffix: " + info.Version);
        Assert.IsNotEmpty(info.Version);
        StringAssert.Matches(info.BuildDate ?? "", new System.Text.RegularExpressions.Regex("^[0-9]{4}-[0-9]{2}-[0-9]{2}$"), "GspBuildDate comes from the targets file");
        Assert.AreEqual("MIT", info.LicenseExpression);
        Assert.AreEqual("Open source under the MIT license.", info.LicenseText);
        Assert.AreEqual(GspAppInfo.DefaultCopyright, info.Copyright);
        CollectionAssert.AreEqual(
            new[] { "Documentation|https://example.test/docs", "Source on GitHub|https://github.com/gameshowpro/Example" },
            info.Links.Select(l => l.Text + "|" + l.Url).ToArray());
    }

    [TestMethod]
    public void Options_override_what_was_derived_and_add_links()
    {
        GspHouseOptions options = new() { ProductName = "Example", Version = "9.9.9", Copyright = "(c) 2030 Someone", LicenseText = "Custom.", LicenseUrl = "https://example.test/LICENSE" };
        options.ExtraLinks.Add(new GspAboutLink("Sponsor", "https://example.test/sponsor"));
        GspAppInfo info = GspAppInfo.Resolve(options, typeof(HouseStyleRegistrationTests).Assembly);
        Assert.AreEqual("Example", info.ProductName);
        Assert.AreEqual("9.9.9", info.Version);
        Assert.AreEqual("(c) 2030 Someone", info.Copyright);
        Assert.AreEqual("Custom.", info.LicenseText);
        CollectionAssert.AreEqual(new[] { "Documentation", "Source on GitHub", "License text", "Sponsor" }, info.Links.Select(l => l.Text).ToArray());
    }

    [TestMethod]
    public void PublicVersion_drops_the_commit_suffix_only()
    {
        Assert.AreEqual("1.2.3", GspAppInfo.PublicVersion("1.2.3+abcdef0"));
        Assert.AreEqual("1.2.3-beta.5", GspAppInfo.PublicVersion("1.2.3-beta.5+abcdef0"));
        Assert.AreEqual("1.2.3", GspAppInfo.PublicVersion("1.2.3"));
    }

    [TestMethod]
    public async Task Header_and_footer_need_no_parameters_and_read_runtime_state()
    {
        await using BunitContext context = CreateContext(o => o.Version = "1.2.3");
        GspRuntimeState state = context.Services.GetRequiredService<GspRuntimeState>();
        state.Platform = "ESP32";
        state.HostName = "fm01-106";
        state.Connection = RemoteServiceStates.Connected;

        IRenderedComponent<GspAppHeader> header = context.Render<GspAppHeader>();
        Assert.AreEqual("GameshowPro.Common.Blazor.Test", header.Find("h1.gsp-app-name").TextContent);
        Assert.AreEqual("images/app-icon.svg", header.Find("img.gsp-app-icon").GetAttribute("src"));
        Assert.AreEqual("ESP32 1.2.3", header.Find(".gsp-pill.gsp-version").TextContent);
        StringAssert.StartsWith(header.Find(".gsp-pill.gsp-version").GetAttribute("title"), "Built ");
        Assert.AreEqual("fm01-106", header.Find(".gsp-host .gsp-data").TextContent);
        Assert.AreEqual("Connected", header.Find(".gsp-connection").TextContent);

        state.Connection = RemoteServiceStates.Warning;
        header.WaitForAssertion(() =>
        {
            Assert.AreEqual("Reconnecting", header.Find(".gsp-connection").TextContent);
            Assert.IsTrue(header.Find(".gsp-connection").ClassList.Contains("warn"));
        });
        state.Connection = RemoteServiceStates.Disconnected;
        header.WaitForAssertion(() =>
        {
            Assert.AreEqual("Disconnected", header.Find(".gsp-connection").TextContent);
            Assert.IsTrue(header.Find(".gsp-connection").ClassList.Contains("bad"));
        });
        state.Connection = null;
        header.WaitForAssertion(() => Assert.IsEmpty(header.FindAll(".gsp-connection")));

        IRenderedComponent<GspPageFooter> footer = context.Render<GspPageFooter>();
        Assert.AreEqual("GameshowPro.Common.Blazor.Test 1.2.3, © 2026 Barjonas LLC", footer.Find("footer span").TextContent);
    }

    [TestMethod]
    public async Task Version_pill_and_footer_open_the_about_dialog_once_one_is_on_the_page()
    {
        await using BunitContext context = CreateContext(o => o.Version = "1.2.3");
        GspRuntimeState state = context.Services.GetRequiredService<GspRuntimeState>();
        IRenderedComponent<GspAppHeader> header = context.Render<GspAppHeader>();
        IRenderedComponent<GspPageFooter> footer = context.Render<GspPageFooter>();
        Assert.IsEmpty(header.FindAll("button.gsp-pill"), "no dialog yet, so the pill is not a button");
        Assert.IsEmpty(footer.FindAll("button"));

        int requests = 0;
        state.AboutRequested += () => requests++;
        IRenderedComponent<GspAboutDialog> dialog = context.Render<GspAboutDialog>();
        header.WaitForAssertion(() => Assert.HasCount(1, header.FindAll("button.gsp-pill")));
        footer.WaitForAssertion(() => Assert.HasCount(1, footer.FindAll("button")));

        header.Find("button.gsp-pill").Click();
        footer.Find("button").Click();
        Assert.AreEqual(2, requests);
        Assert.IsTrue(state.AboutAvailable);

        await context.DisposeComponentsAsync();
        Assert.IsFalse(state.AboutAvailable);
    }

    [TestMethod]
    public async Task About_dialog_shows_the_derived_facts_the_runtime_rows_links_and_legal_line()
    {
        await using BunitContext context = CreateContext(o =>
        {
            o.Tagline = "Does the thing.";
            o.Trademark = "Example is a trademark of Barjonas LLC.";
            o.ExtraLinks.Add(new GspAboutLink("Sponsor", "https://example.test/sponsor"));
        });
        GspRuntimeState state = context.Services.GetRequiredService<GspRuntimeState>();
        state.HostName = "fm01-107";
        state.Platform = "Windows";
        state.SetAboutRow("Sway", "1.2.3");

        IRenderedComponent<GspAboutDialog> cut = context.Render<GspAboutDialog>(p => p.AddChildContent("<p id=\"extra\">extra</p>"));
        Assert.AreEqual("GameshowPro.Common.Blazor.Test", cut.Find("h2#gsp-about-heading").TextContent);
        Assert.AreEqual("Does the thing.", cut.Find(".gsp-about-brand p").TextContent);
        string[] labels = [.. cut.FindAll(".gsp-kv th").Select(e => e.TextContent)];
        CollectionAssert.AreEqual(new[] { "Version", "Build", "Host", "Platform", "Sway" }, labels);
        Assert.AreEqual("fm01-107", cut.FindAll(".gsp-kv td")[2].TextContent);
        StringAssert.Contains(cut.Markup, "Open source under the MIT license.");
        IReadOnlyList<AngleSharp.Dom.IElement> links = cut.FindAll(".gsp-about-links a");
        CollectionAssert.AreEqual(new[] { "Documentation", "Source on GitHub", "Sponsor" }, links.Select(l => l.TextContent).ToArray());
        Assert.IsTrue(links.All(l => l.GetAttribute("target") == "_blank" && l.GetAttribute("rel") == "noopener noreferrer"));
        Assert.AreEqual("Example is a trademark of Barjonas LLC. © 2026 Barjonas LLC.", cut.FindAll("p.gsp-hint")[^1].TextContent);
        Assert.AreEqual("extra", cut.Find("#extra").TextContent);
        StringAssert.Contains(cut.Find(".gsp-about-credit a").GetAttribute("href"), "github.com/gameshowpro");
        Assert.IsNotNull(cut.Find(".gsp-about-credit svg.gsp-logo"));
        Assert.AreEqual("dialog", cut.Find("form.gsp-dialog-foot").GetAttribute("method"));
    }

    [TestMethod]
    public async Task About_dialog_opens_through_the_runtime_state_request()
    {
        await using BunitContext context = CreateContext();
        BunitJSModuleInterop module = context.JSInterop.SetupModule("./_content/GameshowPro.Common.Blazor/gsp-dialog.js");
        module.SetupVoid("showModal", _ => true).SetVoidResult();
        context.Render<GspAboutDialog>();
        context.Services.GetRequiredService<GspRuntimeState>().RequestAbout();
        module.VerifyInvoke("showModal");
    }

    [TestMethod]
    public async Task App_shell_renders_the_whole_frame_from_the_registration_alone()
    {
        await using BunitContext context = CreateContext(o => o.SecondaryAccent = HouseThemeContrastTests.LogoBlue);
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>(p => p.AddChildContent("<p id=\"page\">content</p>"));
        Assert.IsNotNull(shell.Find("header.gsp-header"));
        Assert.AreEqual("content", shell.Find("main.gsp-main #page").TextContent);
        Assert.IsNotNull(shell.Find("footer.gsp-footer"));
        Assert.IsNotNull(shell.Find("dialog.gsp-dialog"));
        Assert.IsNotNull(shell.Find("button.gsp-about-button"));
        StringAssert.Contains(shell.Markup, "--gsp-accent-2:");
    }

    [TestMethod]
    public async Task App_shell_hides_the_about_button_when_the_registration_says_so()
    {
        await using BunitContext context = CreateContext(o => o.ShowAboutButton = false);
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>();
        Assert.IsEmpty(shell.FindAll("button.gsp-about-button"));
        Assert.IsNotNull(shell.Find("dialog.gsp-dialog"));
    }

    [TestMethod]
    public async Task Theme_provider_takes_both_accents_from_the_registration()
    {
        await using BunitContext context = CreateContext(o =>
        {
            o.Accent = HouseThemeContrastTests.LogoYellow;
            o.SecondaryAccent = HouseThemeContrastTests.LogoBlue;
        });
        string light = context.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, false)).Markup;
        StringAssert.Contains(light, "--gsp-accent:#ffe600;--gsp-accent-ink:#16191d;--gsp-accent-2:#2e6fb7;--gsp-accent-2-ink:#ffffff;");
        StringAssert.Contains(light, "--mud-palette-secondary: rgba(46,111,183,1)");
        string dark = context.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, true)).Markup;
        StringAssert.Contains(dark, "--gsp-accent-2:#5c96d6;--gsp-accent-2-ink:#13151a;");
    }

    [TestMethod]
    public async Task Theme_provider_without_a_second_accent_writes_the_neutral()
    {
        await using BunitContext context = CreateContext();
        string light = context.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, false)).Markup;
        StringAssert.Contains(light, "--gsp-accent-2:#667085;--gsp-accent-2-ink:#ffffff;");
        StringAssert.Contains(light, "--mud-palette-secondary: rgba(102,112,133,1)");
    }

    // Regression: the provider used to turn MudBlazor's observer off, which is the only thing that attaches the browser's
    // prefers-color-scheme change listener, so an OS or DevTools switch was never reported until a reload.
    [TestMethod]
    public async Task Theme_provider_follows_a_live_system_change_and_a_forced_mode_does_not_listen()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspThemeProvider> cut = context.Render<GspThemeProvider>();
        Assert.IsTrue(context.JSInterop.Invocations.Any(i => i.Identifier == "mudThemeProvider.watchDarkMode"), "the JS listener is attached");
        StringAssert.Contains(cut.Markup, "color-scheme:light");

        bool? reported = null;
        cut.Render(p => p.Add(x => x.IsDarkModeChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<bool>(this, v => reported = v)));
        MudThemeProvider mud = cut.FindComponent<MudThemeProvider>().Instance;
        await cut.InvokeAsync(() => mud.SystemDarkModeChangedAsync(true));
        cut.WaitForAssertion(() =>
        {
            StringAssert.Contains(cut.Markup, "color-scheme:dark");
            StringAssert.Contains(cut.Markup, "--mud-palette-background: rgba(19,21,26,1)");
            StringAssert.Contains(cut.Markup, "--gsp-shadow:none");
        });
        Assert.AreEqual(true, reported);
        await cut.InvokeAsync(() => mud.SystemDarkModeChangedAsync(false));
        cut.WaitForAssertion(() => StringAssert.Contains(cut.Markup, "color-scheme:light"));

        await using BunitContext forcedContext = CreateContext();
        forcedContext.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, true));
        Assert.IsFalse(forcedContext.JSInterop.Invocations.Any(i => i.Identifier == "mudThemeProvider.watchDarkMode"), "a forced mode attaches no listener");
    }

    [TestMethod]
    public async Task Connection_overlay_shows_the_logo_ring_and_a_product_title_from_the_registration()
    {
        await using BunitContext context = CreateContext();
        ServiceState state = new("service", "service", RemoteServiceStates.Disconnected, "gone", null, null);
        IRenderedComponent<ServiceConnectionOverlay> cut = context.Render<ServiceConnectionOverlay>(p => p.Add(x => x.DataContext, state));
        Assert.AreEqual("Reconnecting to GameshowPro.Common.Blazor.Test...", cut.Find("#gsp-connection-title").TextContent);
        Assert.AreEqual("images/app-icon.svg", cut.Find(".gsp-ring img.gsp-ring-logo").GetAttribute("src"));
        Assert.IsNotNull(cut.Find(".gsp-ring.indeterminate"));
    }

    [TestMethod]
    public async Task Connection_overlay_without_a_registration_keeps_its_published_defaults()
    {
        await using BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMudServices();
        ServiceState state = new("service", "service", RemoteServiceStates.Disconnected, "gone", null, null);

        IRenderedComponent<ServiceConnectionOverlay> cut = context.Render<ServiceConnectionOverlay>(p => p.Add(x => x.DataContext, state));
        Assert.AreEqual("Disconnected from service", cut.Find("#gsp-connection-title").TextContent);
        Assert.IsEmpty(cut.FindAll("img"));
        Assert.IsNotNull(cut.Find(".gsp-ring.small"));

        IRenderedComponent<ServiceConnectionOverlay> named = context.Render<ServiceConnectionOverlay>(p => p
            .Add(x => x.DataContext, state).Add(x => x.ProductName, "Soundling").Add(x => x.IconUrl, "logo.svg"));
        Assert.AreEqual("Reconnecting to Soundling...", named.Find("#gsp-connection-title").TextContent);
        Assert.AreEqual("logo.svg", named.Find("img.gsp-ring-logo").GetAttribute("src"));

        IRenderedComponent<ServiceConnectionOverlay> titled = context.Render<ServiceConnectionOverlay>(p => p
            .Add(x => x.DataContext, state).Add(x => x.ProductName, "Soundling").Add(x => x.Title, "Disconnected from Soundling service"));
        Assert.AreEqual("Disconnected from Soundling service", titled.Find("#gsp-connection-title").TextContent);
    }
}
