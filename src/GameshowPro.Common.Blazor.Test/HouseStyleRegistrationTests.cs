namespace GameshowPro.Common.Blazor.Test;

/// <summary>
/// The one-registration design: what <c>AddGspHouseStyle</c> derives from the app's assembly (this test assembly stands in
/// for an app; its csproj imports the package's props file, which is how the build date and license expression arrive),
/// and the components that read the registration by injection with no parameters.
/// </summary>
[TestClass]
public partial class HouseStyleRegistrationTests
{
    [GeneratedRegex("^[0-9]{4}-[0-9]{2}-[0-9]{2}$")]
    private static partial Regex BuildDateRegex();

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
        Assert.MatchesRegex(BuildDateRegex(), info.BuildDate ?? "", "GspBuildDate comes from the props file");
        Assert.IsNotNull(info.BuildInstantUtc, "GspBuildInstantUtc carries a full UTC build timestamp");
        Assert.AreEqual(TimeSpan.Zero, info.BuildInstantUtc.Value.Offset);
        Assert.AreEqual("MIT", info.LicenseExpression);
        Assert.AreEqual("Open source under the MIT license.", info.LicenseText);
        Assert.AreEqual(GspAppInfo.DefaultCopyright, info.Copyright);
        Assert.AreSequenceEqual<string>(
            ["Documentation|https://example.test/docs", "Source on GitHub|https://github.com/gameshowpro/Example"],
            info.Links.Select(l => l.Text + "|" + l.Url));
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
        Assert.AreSequenceEqual<string>(["Documentation", "Source on GitHub", "License text", "Sponsor"], info.Links.Select(l => l.Text));
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
        string? versionTitle = header.Find(".gsp-pill.gsp-version").GetAttribute("title");
        Assert.IsNotNull(versionTitle);
        Assert.AreEqual("Build time unavailable", versionTitle, "the browser zone is unavailable in this test host");
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
    public async Task Local_time_formatter_uses_the_browser_zone_and_per_instant_dst_offsets()
    {
        await using BunitContext context = CreateContext();
        BunitJSModuleInterop module = context.JSInterop.SetupModule("./_content/GameshowPro.Common.Blazor/gsp-local-time.js");
        module.Setup<string[]>("describe", _ => true).SetResult(["America/Los_Angeles", "-420", "-480"]);
        GspLocalTimeFormatter formatter = context.Services.GetRequiredService<GspLocalTimeFormatter>();
        DateTimeOffset first = new(2026, 11, 1, 8, 30, 0, TimeSpan.Zero);
        DateTimeOffset second = first.AddHours(1);

        await formatter.PrimeAsync([first, second]);

        Assert.AreEqual("2026-11-01 01:30:00 UTC-07:00 (America/Los_Angeles)", formatter.FormatCached(first));
        Assert.AreEqual("2026-11-01 01:30:00 UTC-08:00 (America/Los_Angeles)", formatter.FormatCached(second));
        Assert.AreEqual("UTC-07:00", formatter.OffsetCached(first));
        Assert.AreEqual("UTC-08:00", formatter.OffsetCached(second));
        module.VerifyInvoke("describe");
    }

    [TestMethod]
    public async Task Repriming_retained_instants_does_not_evict_them_at_cache_capacity()
    {
        await using GspLocalTimeFormatter formatter = new(new LocalTimeJs());
        DateTimeOffset[] retained = [.. Enumerable.Range(0, 5000).Select(i => DateTimeOffset.UnixEpoch.AddSeconds(i))];
        DateTimeOffset[] filler = [.. Enumerable.Range(5000, 5000).Select(i => DateTimeOffset.UnixEpoch.AddSeconds(i))];
        DateTimeOffset[] newRows = [.. Enumerable.Range(10000, 5000).Select(i => DateTimeOffset.UnixEpoch.AddSeconds(i))];

        await formatter.PrimeAsync(retained);
        await formatter.PrimeAsync(filler);
        await formatter.PrimeAsync(retained.Concat(newRows));

        Assert.IsTrue(retained.All(instant => formatter.FormatCached(instant) is not null));
        Assert.IsTrue(newRows.All(instant => formatter.FormatCached(instant) is not null));
    }

    // This in-memory test fake returns data directly; no browser interop occurs here.
#pragma warning disable BL0016
    private sealed class LocalTimeJs : IJSRuntime, IJSObjectReference
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => Respond<TValue>(identifier, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => Respond<TValue>(identifier, args);

        private ValueTask<TValue> Respond<TValue>(string identifier, object?[]? args)
        {
            object result = identifier switch
            {
                "import" => this,
                "describe" => new[] { "Etc/UTC" }.Concat(Enumerable.Repeat("0", ((string[])args![0]!).Length)).ToArray(),
                _ => throw new InvalidOperationException(identifier),
            };
            return ValueTask.FromResult((TValue)result);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
#pragma warning restore BL0016

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
        Assert.AreSequenceEqual<string>(["Version", "Build", "Host", "Platform", "Sway"], labels);
        Assert.AreEqual("fm01-107", cut.FindAll(".gsp-kv td")[2].TextContent);
        Assert.Contains("Open source under the MIT license.", cut.Markup);
        IReadOnlyList<AngleSharp.Dom.IElement> links = cut.FindAll(".gsp-about-links a");
        Assert.AreSequenceEqual<string>(["Documentation", "Source on GitHub", "Sponsor"], links.Select(l => l.TextContent));
        Assert.IsTrue(links.All(l => l.GetAttribute("target") == "_blank" && l.GetAttribute("rel") == "noopener noreferrer"));
        Assert.AreEqual("Example is a trademark of Barjonas LLC. © 2026 Barjonas LLC.", cut.FindAll("p.gsp-hint")[^1].TextContent);
        Assert.AreEqual("extra", cut.Find("#extra").TextContent);
        string? creditHref = cut.Find(".gsp-about-credit a").GetAttribute("href");
        Assert.IsNotNull(creditHref);
        Assert.Contains("github.com/gameshowpro", creditHref);
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
        await using BunitContext context = CreateContext(o => o.SecondaryAccent = HouseThemeContrastTests.s_logoBlue);
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>(p => p.AddChildContent("<p id=\"page\">content</p>"));
        Assert.IsNotNull(shell.Find("header.gsp-header"));
        Assert.AreEqual("content", shell.Find("main.gsp-main #page").TextContent);
        Assert.IsEmpty(shell.FindAll("footer.gsp-footer"));
        Assert.IsNotNull(shell.Find("dialog.gsp-dialog"));
        Assert.IsNotNull(shell.Find("button.gsp-about-button"));
        Assert.Contains("--gsp-accent-2:", shell.Markup);
    }

    [TestMethod]
    public async Task App_shell_shows_the_footer_only_when_the_registration_asks_for_it()
    {
        await using BunitContext context = CreateContext(o => o.ShowFooter = true);
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>();
        Assert.IsNotNull(shell.Find("footer.gsp-footer"));
    }

    [TestMethod]
    public async Task App_shell_covers_the_page_while_the_connection_is_not_connected()
    {
        await using BunitContext context = CreateContext(o => o.ShowFooter = false);
        GspRuntimeState state = context.Services.GetRequiredService<GspRuntimeState>();
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>();
        Assert.IsEmpty(shell.FindAll(".gsp-connection-card"), "No connection reported: no overlay.");
        foreach (RemoteServiceStates lost in new[] { RemoteServiceStates.Warning, RemoteServiceStates.Disconnected })
        {
            state.Connection = lost;
            shell.WaitForAssertion(() => Assert.IsNotNull(shell.Find(".gsp-connection-card")));
            state.Connection = RemoteServiceStates.Connected;
            shell.WaitForAssertion(() => Assert.IsEmpty(shell.FindAll(".gsp-connection-card")));
        }
    }

    [TestMethod]
    [DataRow("Copyright (c) 2026 Barjonas LLC", "(c) 2026 Barjonas LLC")]
    [DataRow("Copyright © 2026 Barjonas LLC", "© 2026 Barjonas LLC")]
    [DataRow("2026 Barjonas LLC", "© 2026 Barjonas LLC")]
    [DataRow("© 2026 Barjonas LLC", "© 2026 Barjonas LLC")]
    public void Copyright_gets_exactly_one_mark(string input, string expected)
        => Assert.AreEqual(expected, GspAppInfo.NormalizeCopyright(input));

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
            o.Accent = HouseThemeContrastTests.s_logoYellow;
            o.SecondaryAccent = HouseThemeContrastTests.s_logoBlue;
        });
        string light = context.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, false)).Markup;
        Assert.Contains("--gsp-accent:#ffe600;--gsp-accent-ink:#16191d;--gsp-accent-2:#2e6fb7;--gsp-accent-2-ink:#ffffff;", light);
        Assert.Contains("--mud-palette-secondary: rgba(46,111,183,1)", light);
        string dark = context.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, true)).Markup;
        Assert.Contains("--gsp-accent-2:#5c96d6;--gsp-accent-2-ink:#13151a;", dark);
    }

    [TestMethod]
    public async Task Theme_provider_without_a_second_accent_writes_the_neutral()
    {
        await using BunitContext context = CreateContext();
        string light = context.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, false)).Markup;
        Assert.Contains("--gsp-accent-2:#667085;--gsp-accent-2-ink:#ffffff;", light);
        Assert.Contains("--mud-palette-secondary: rgba(102,112,133,1)", light);
    }

    // Regression: the provider used to turn MudBlazor's observer off, which is the only thing that attaches the browser's
    // prefers-color-scheme change listener, so an OS or DevTools switch was never reported until a reload.
    [TestMethod]
    public async Task Theme_provider_follows_a_live_system_change_and_a_forced_mode_does_not_listen()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspThemeProvider> cut = context.Render<GspThemeProvider>();
        Assert.Contains(i => i.Identifier == "mudThemeProvider.watchDarkMode", context.JSInterop.Invocations, "the JS listener is attached");
        Assert.Contains("color-scheme:light", cut.Markup);

        bool? reported = null;
        cut.Render(p => p.Add(x => x.IsDarkModeChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<bool>(this, v => reported = v)));
        MudThemeProvider mud = cut.FindComponent<MudThemeProvider>().Instance;
        await cut.InvokeAsync(() => mud.SystemDarkModeChangedAsync(true));
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("color-scheme:dark", cut.Markup);
            Assert.Contains("--mud-palette-background: rgba(19,21,26,1)", cut.Markup);
            Assert.Contains("--gsp-shadow:none", cut.Markup);
        });
        Assert.IsTrue(reported);
        await cut.InvokeAsync(() => mud.SystemDarkModeChangedAsync(false));
        cut.WaitForAssertion(() => Assert.Contains("color-scheme:light", cut.Markup));

        await using BunitContext forcedContext = CreateContext();
        forcedContext.Render<GspThemeProvider>(p => p.Add(x => x.ForceDarkMode, true));
        Assert.DoesNotContain(i => i.Identifier == "mudThemeProvider.watchDarkMode", forcedContext.JSInterop.Invocations, "a forced mode attaches no listener");
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
