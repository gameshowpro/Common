
namespace GameshowPro.Common.Blazor.Test;

/// <summary>
/// The generic pieces ported from Soundling: <see cref="GspChoiceGroup{TItem}"/>, <see cref="GspConfirmDialog"/>,
/// <see cref="GspCollapsibleSection"/>, <see cref="GspClipboard"/> and the stylesheet rules that go with them.
/// </summary>
[TestClass]
public class HouseMoreComponentsTests
{
    private const string StorageModule = "./_content/GameshowPro.Common.Blazor/gsp-storage.js";
    private const string ClipboardModule = "./_content/GameshowPro.Common.Blazor/gsp-clipboard.js";

    private enum Direction
    {
        Up,
        Down,
    }

    private static BunitContext CreateContext()
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMudServices();
        return context;
    }

    // ---- GspChoiceGroup -------------------------------------------------------------------------------------------

    private static string Label(Direction direction) => direction == Direction.Up ? "Count up" : "Count down";

    private static IRenderedComponent<GspChoiceGroup<Direction>> RenderGroup(BunitContext context, Direction value, Action<Direction>? changed = null)
        => context.Render<GspChoiceGroup<Direction>>(p => p
            .Add(x => x.Items, Enum.GetValues<Direction>())
            .Add(x => x.Text, Label)
            .Add(x => x.Label, "Time direction")
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<Direction>(context, d => changed?.Invoke(d))));

    [TestMethod]
    public async Task ChoiceGroup_renders_one_button_per_item_in_a_group_with_an_accessible_name()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspChoiceGroup<Direction>> group = RenderGroup(context, Direction.Up);
        Assert.AreSequenceEqual<string>(["Count up", "Count down"], group.FindAll(".mud-toggle-item").Select(i => i.TextContent.Trim()));
        Assert.AreEqual("Time direction", group.Find(".mud-toggle-group").GetAttribute("aria-label"));
    }

    [TestMethod]
    public async Task ChoiceGroup_marks_only_the_current_value_selected()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspChoiceGroup<Direction>> group = RenderGroup(context, Direction.Down);
        IReadOnlyList<IElement> selected = group.FindAll(".mud-toggle-item.mud-toggle-item-selected");
        Assert.HasCount(1, selected);
        Assert.AreEqual("Count down", selected[0].TextContent.Trim());
    }

    [TestMethod]
    public async Task ChoiceGroup_click_raises_ValueChanged_with_that_item()
    {
        await using BunitContext context = CreateContext();
        List<Direction> raised = [];
        IRenderedComponent<GspChoiceGroup<Direction>> group = RenderGroup(context, Direction.Up, raised.Add);
        await group.InvokeAsync(() => group.FindAll(".mud-toggle-item").Single(i => i.TextContent.Contains("Count down")).Click());
        Assert.AreSequenceEqual([Direction.Down], raised);
    }

    /// <summary>A host that owns the value and takes a click, or refuses it (the value stays as it was).</summary>
    private sealed class RefusingHost : ComponentBase
    {
        [Parameter]
        public Direction Value { get; set; }

        [Parameter]
        public bool Refuse { get; set; }

        public List<Direction> Calls { get; } = [];

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<GspChoiceGroup<Direction>>(0);
            builder.AddComponentParameter(1, nameof(GspChoiceGroup<>.Items), Enum.GetValues<Direction>());
            builder.AddComponentParameter(2, nameof(GspChoiceGroup<>.Text), (Func<Direction, string>)Label);
            builder.AddComponentParameter(3, nameof(GspChoiceGroup<>.Value), Value);
            builder.AddComponentParameter(4, nameof(GspChoiceGroup<>.ValueChanged), EventCallback.Factory.Create<Direction>(this, value =>
            {
                Calls.Add(value);
                if (!Refuse)
                {
                    Value = value;
                }
            }));
            builder.CloseComponent();
        }
    }

    private static string Pressed(IRenderedComponent<RefusingHost> host)
        => host.Find(".mud-toggle-item.mud-toggle-item-selected").TextContent.Trim();

    private static Task Click(IRenderedComponent<RefusingHost> host, string text)
        => host.InvokeAsync(() => host.FindAll(".mud-toggle-item").Single(i => i.TextContent.Trim() == text).Click());

    [TestMethod]
    public async Task ChoiceGroup_a_refused_click_shows_the_hosts_value_again_and_can_be_retried()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<RefusingHost> host = context.Render<RefusingHost>(p => p.Add(x => x.Value, Direction.Up).Add(x => x.Refuse, true));

        await Click(host, "Count down");
        Assert.AreSequenceEqual([Direction.Down], host.Instance.Calls);
        host.WaitForAssertion(() => Assert.AreEqual("Count up", Pressed(host)));

        // The same choice can be tried again, and once the host takes it, it stays pressed.
        await Click(host, "Count down");
        Assert.AreSequenceEqual([Direction.Down, Direction.Down], host.Instance.Calls);
        host.WaitForAssertion(() => Assert.AreEqual("Count up", Pressed(host)));
        host.Render(p => p.Add(x => x.Refuse, false));
        await Click(host, "Count down");
        host.WaitForAssertion(() => Assert.AreEqual("Count down", Pressed(host)));
    }

    [TestMethod]
    public async Task ChoiceGroup_an_accepted_click_keeps_the_same_toggle_group_so_focus_stays()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<RefusingHost> host = context.Render<RefusingHost>(p => p.Add(x => x.Value, Direction.Up));
        MudToggleGroup<Direction> group = host.FindComponent<MudToggleGroup<Direction>>().Instance;

        await Click(host, "Count down");

        Assert.AreSame(group, host.FindComponent<MudToggleGroup<Direction>>().Instance);
        Assert.AreEqual("Count down", Pressed(host));
    }

    [TestMethod]
    public async Task ChoiceGroup_label_is_only_a_caption_when_asked()
    {
        await using BunitContext context = CreateContext();
        Assert.IsEmpty(RenderGroup(context, Direction.Up).FindAll(".gsp-choice-group > .mud-typography"));
        IRenderedComponent<GspChoiceGroup<Direction>> shown = context.Render<GspChoiceGroup<Direction>>(p => p
            .Add(x => x.Items, Enum.GetValues<Direction>())
            .Add(x => x.Text, Label)
            .Add(x => x.Label, "Time direction")
            .Add(x => x.ShowLabel, true)
            .Add(x => x.Value, Direction.Up));
        Assert.AreEqual("Time direction", shown.Find(".gsp-choice-group > .mud-typography").TextContent.Trim());
    }

    [TestMethod]
    public async Task ChoiceGroup_title_gives_each_button_a_tooltip_and_ItemDisabled_disables_one()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspChoiceGroup<Direction>> group = context.Render<GspChoiceGroup<Direction>>(p => p
            .Add(x => x.Items, Enum.GetValues<Direction>())
            .Add(x => x.Text, Label)
            .Add(x => x.Title, d => "Tip " + d)
            .Add(x => x.ItemDisabled, d => d == Direction.Down)
            .Add(x => x.Value, Direction.Up));
        Assert.AreSequenceEqual<string?>(["Tip Up", "Tip Down"], group.FindAll(".mud-toggle-item span[title]").Select(s => s.GetAttribute("title")));
        Assert.HasCount(1, group.FindAll(".mud-toggle-item[disabled]"));
    }

    // ---- GspConfirmDialog -----------------------------------------------------------------------------------------

    private sealed class DialogFixture : IAsyncDisposable
    {
        public BunitContext Context { get; } = CreateContext();

        public IRenderedComponent<MudDialogProvider> Dialogs { get; }

        public IDialogService Service { get; }

        public DialogFixture()
        {
            Context.Render<MudPopoverProvider>();
            Dialogs = Context.Render<MudDialogProvider>();
            Service = Context.Services.GetRequiredService<IDialogService>();
        }

        public string[] FocusCalls => [.. Context.JSInterop.Invocations
            .Select(call => call.Identifier)
            .Where(id => id is "mudElementRef.focusFirst" or "mudElementRef.focusLast" or "Blazor._internal.domWrapper.focus")];

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    [TestMethod]
    public async Task ConfirmDialog_puts_Cancel_first_and_focuses_the_first_tabbable_element()
    {
        await using DialogFixture f = new();

        Task<bool> answer = f.Dialogs.InvokeAsync(() => GspConfirmDialog.Ask(f.Service, "Delete page", "Delete 'x'?", "Delete"));
        f.Dialogs.WaitForAssertion(() => Assert.IsNotEmpty(f.Dialogs.FindAll("[data-testid=confirm-dialog]")));

        f.Dialogs.WaitForAssertion(() => Assert.Contains("mudElementRef.focusFirst", f.FocusCalls));
        Assert.DoesNotContain("mudElementRef.focusLast", f.FocusCalls);
        Assert.AreSequenceEqual<string?>(["confirm-cancel", "confirm-yes"], f.Dialogs.FindAll(".mud-dialog-actions button").Select(b => b.GetAttribute("data-testid")));
        Assert.AreEqual("Cancel", f.Dialogs.Find("[data-testid=confirm-cancel]").TextContent.Trim());
        Assert.AreEqual("Delete", f.Dialogs.Find("[data-testid=confirm-yes]").TextContent.Trim());
        Assert.Contains("Delete 'x'?", f.Dialogs.Find("[data-testid=confirm-message]").TextContent);

        await f.Dialogs.Find("[data-testid=confirm-cancel]").ClickAsync(new MouseEventArgs());
        Assert.IsFalse(await answer);
    }

    [TestMethod]
    public async Task ConfirmDialog_answers_yes_only_from_the_confirming_button()
    {
        await using DialogFixture f = new();

        Task<bool> answer = f.Dialogs.InvokeAsync(() => GspConfirmDialog.Ask(f.Service, "Replace audio", "Replace it?", "Replace"));
        f.Dialogs.WaitForAssertion(() => Assert.IsNotEmpty(f.Dialogs.FindAll("[data-testid=confirm-yes]")));

        await f.Dialogs.Find("[data-testid=confirm-yes]").ClickAsync(new MouseEventArgs());
        Assert.IsTrue(await answer);
    }

    [TestMethod]
    public async Task ConfirmDialog_uses_a_custom_cancel_text_and_keeps_the_message_box_class()
    {
        await using DialogFixture f = new();

        _ = f.Dialogs.InvokeAsync(() => GspConfirmDialog.Ask(f.Service, "Delete page", "Delete 'x'?", "Delete", "Keep"));
        f.Dialogs.WaitForAssertion(() => Assert.IsNotEmpty(f.Dialogs.FindAll(".mud-message-box .mud-dialog-content")));
        Assert.AreEqual("Keep", f.Dialogs.Find("[data-testid=confirm-cancel]").TextContent.Trim());
    }

    // ---- GspCollapsibleSection ------------------------------------------------------------------------------------

    private static IRenderedComponent<GspCollapsibleSection> RenderSection(BunitContext context, Action<ComponentParameterCollectionBuilder<GspCollapsibleSection>>? more = null, string? storageKey = "app.subsection.output")
        => context.Render<GspCollapsibleSection>(p =>
        {
            p.Add(x => x.Title, "Output")
             .Add(x => x.StorageKey, storageKey)
             .Add(x => x.Summary, "Out 0 at 0 dB")
             .AddChildContent("<span class=\"body-probe\">body</span>");
            more?.Invoke(p);
        });

    private static bool IsOpen(IRenderedComponent<GspCollapsibleSection> cut) => cut.Find("button.gsp-collapsible-toggle").GetAttribute("aria-expanded") == "true";

    private static Task Toggle(IRenderedComponent<GspCollapsibleSection> cut) => cut.InvokeAsync(() => cut.Find("button.gsp-collapsible-toggle").Click());

    [TestMethod]
    public async Task Collapsible_renders_an_h3_with_a_toggle_button_and_the_storage_key_attribute()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspCollapsibleSection> cut = RenderSection(context);
        Assert.IsNotNull(cut.Find("h3 > button.gsp-collapsible-toggle[aria-expanded]"));
        Assert.AreEqual("Output", cut.Find(".gsp-collapsible-title").TextContent);
        Assert.AreEqual("app.subsection.output", cut.Find(".gsp-collapsible").GetAttribute("data-gsp-section"));

        IRenderedComponent<GspCollapsibleSection> h4 = RenderSection(context, p => p.Add(x => x.HeadingTag, "h4"));
        Assert.IsNotNull(h4.Find("h4 > button.gsp-collapsible-toggle"));
        Assert.IsEmpty(h4.FindAll("h3"));
    }

    [TestMethod]
    public async Task Collapsible_starts_closed_with_a_summary_and_no_body_unless_DefaultOpen()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspCollapsibleSection> closed = RenderSection(context);
        Assert.IsFalse(IsOpen(closed));
        Assert.AreEqual("Out 0 at 0 dB", closed.Find(".gsp-collapsible-summary").TextContent.Trim());
        Assert.IsEmpty(closed.FindAll(".body-probe"));
        Assert.IsNull(closed.Find("button.gsp-collapsible-toggle").GetAttribute("aria-controls"));

        IRenderedComponent<GspCollapsibleSection> open = RenderSection(context, p => p.Add(x => x.DefaultOpen, true));
        Assert.IsTrue(IsOpen(open));
        Assert.IsEmpty(open.FindAll(".gsp-collapsible-summary"));
        Assert.HasCount(1, open.FindAll(".gsp-collapsible-body .body-probe"));
    }

    [TestMethod]
    public async Task Collapsible_SummaryContent_replaces_the_plain_summary_while_closed()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspCollapsibleSection> cut = RenderSection(context, p => p.Add(x => x.SummaryContent, (RenderFragment)(b => b.AddMarkupContent(0, "<em class=\"custom-summary\">custom</em>"))));
        Assert.HasCount(1, cut.FindAll(".custom-summary"));
        Assert.IsEmpty(cut.FindAll("span.gsp-collapsible-summary"));
        await Toggle(cut);
        Assert.IsEmpty(cut.FindAll(".custom-summary"));
    }

    [TestMethod]
    public async Task Collapsible_toggling_opens_and_closes_the_body_and_remembers_the_choice()
    {
        await using BunitContext context = CreateContext();
        BunitJSModuleInterop module = context.JSInterop.SetupModule(StorageModule);
        IRenderedComponent<GspCollapsibleSection> cut = RenderSection(context);

        await Toggle(cut);
        Assert.IsTrue(IsOpen(cut));
        Assert.IsNotNull(cut.Find("button.gsp-collapsible-toggle").GetAttribute("aria-controls"));
        Assert.HasCount(1, cut.FindAll(".body-probe"));
        JSRuntimeInvocation open = module.Invocations.Last(call => call.Identifier == "set");
        Assert.AreEqual("app.subsection.output", open.Arguments[0]);
        Assert.AreEqual("open", open.Arguments[1]);

        await Toggle(cut);
        Assert.IsFalse(IsOpen(cut));
        Assert.AreEqual("closed", module.Invocations.Last(call => call.Identifier == "set").Arguments[1]);
    }

    [TestMethod]
    public async Task Collapsible_a_stored_choice_overrides_the_default()
    {
        await using BunitContext context = CreateContext();
        context.JSInterop.SetupModule(StorageModule).Setup<string?>("get", "app.subsection.output").SetResult("open");
        Assert.IsTrue(IsOpen(RenderSection(context)));

        await using BunitContext other = CreateContext();
        other.JSInterop.SetupModule(StorageModule).Setup<string?>("get", "app.subsection.output").SetResult("closed");
        Assert.IsFalse(IsOpen(RenderSection(other, p => p.Add(x => x.DefaultOpen, true))));
    }

    [TestMethod]
    public async Task Collapsible_without_a_key_does_not_touch_storage_and_still_toggles()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspCollapsibleSection> cut = RenderSection(context, storageKey: null);
        await Toggle(cut);
        Assert.IsTrue(IsOpen(cut));
        Assert.IsEmpty(context.JSInterop.Invocations.Where(call => call.Identifier == "import"));
    }

    [TestMethod]
    public async Task Collapsible_with_failing_storage_or_a_failing_import_keeps_the_default_and_toggle_still_works()
    {
        await using BunitContext blocked = CreateContext();
        BunitJSModuleInterop module = blocked.JSInterop.SetupModule(StorageModule);
        module.Setup<string?>(call => call.Identifier == "get").SetException(new JSException("storage blocked"));
        module.SetupVoid(call => call.Identifier == "set").SetException(new JSException("storage blocked"));
        IRenderedComponent<GspCollapsibleSection> cut = RenderSection(blocked, p => p.Add(x => x.DefaultOpen, true));
        Assert.IsTrue(IsOpen(cut));
        await Toggle(cut);
        Assert.IsFalse(IsOpen(cut));

        // The module itself cannot be imported (strict mode with no module set up).
        await using BunitContext noModule = new();
        noModule.JSInterop.Mode = JSRuntimeMode.Strict;
        noModule.Services.AddMudServices();
        IRenderedComponent<GspCollapsibleSection> cut2 = RenderSection(noModule);
        Assert.IsFalse(IsOpen(cut2));
        await Toggle(cut2);
        Assert.IsTrue(IsOpen(cut2));
    }

    // ---- GspClipboard ---------------------------------------------------------------------------------------------

    [TestMethod]
    public async Task Clipboard_calls_the_module_with_the_exact_text_and_returns_its_result()
    {
        await using BunitContext context = CreateContext();
        BunitJSModuleInterop module = context.JSInterop.SetupModule(ClipboardModule);
        module.Setup<bool>("copy", "/cues/intro/play").SetResult(true);
        module.Setup<bool>("copy", "/a").SetResult(false);
        GspClipboard clipboard = new(context.Services.GetRequiredService<IJSRuntime>());

        Assert.IsTrue(await clipboard.CopyAsync("/cues/intro/play"));
        Assert.IsFalse(await clipboard.CopyAsync("/a"));
        JSRuntimeInvocation call = module.Invocations.First(c => c.Identifier == "copy");
        Assert.AreEqual("/cues/intro/play", call.Arguments.Single());
    }

    [TestMethod]
    public async Task Clipboard_never_throws_when_interop_fails_or_the_module_is_missing()
    {
        await using BunitContext failing = CreateContext();
        failing.JSInterop.SetupModule(ClipboardModule).Setup<bool>("copy", "/b").SetException(new JSException("blocked"));
        Assert.IsFalse(await new GspClipboard(failing.Services.GetRequiredService<IJSRuntime>()).CopyAsync("/b"));

        await using BunitContext missing = new();
        missing.JSInterop.Mode = JSRuntimeMode.Strict;
        Assert.IsFalse(await new GspClipboard(missing.Services.GetRequiredService<IJSRuntime>()).CopyAsync("/c"));
    }

    [TestMethod]
    public async Task AddGspHouseStyle_registers_the_clipboard_and_does_not_replace_an_app_registration()
    {
        await using BunitContext context = CreateContext();
        IJSRuntime js = context.Services.GetRequiredService<IJSRuntime>();

        ServiceCollection services = new();
        services.AddSingleton(js);
        services.AddGspHouseStyle();
        await using ServiceProvider provider = services.BuildServiceProvider();
        Assert.IsNotNull(provider.GetRequiredService<GspClipboard>());

        GspClipboard mine = new(js);
        ServiceCollection own = new();
        own.AddSingleton(js);
        own.AddSingleton(mine);
        own.AddGspHouseStyle();
        await using ServiceProvider ownProvider = own.BuildServiceProvider();
        Assert.AreSame(mine, ownProvider.GetRequiredService<GspClipboard>());
    }

    // ---- Stylesheet -----------------------------------------------------------------------------------------------

    private static string ReadCss()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "src", "GameshowPro.Common.Blazor", "wwwroot", "gsp-house-base.css");
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }
        }
        throw new FileNotFoundException("gsp-house-base.css not found above " + AppContext.BaseDirectory);
    }

    [TestMethod]
    public void Stylesheet_wraps_the_choice_group_and_never_shrinks_a_button_below_its_label()
    {
        string css = ReadCss();
        Assert.DoesNotContain("::deep", css);
        // MudToggleGroup's inline grid of 1fr columns must be replaced by a wrapping flex row.
        Assert.Contains(".gsp-choice-group .mud-toggle-group", css);
        Assert.Contains("display: flex !important", css);
        Assert.Contains("grid-template-columns: none !important", css);
        Assert.Contains("flex-wrap: wrap", css);
        Assert.Contains("flex: 0 0 auto", css);
        Assert.Contains("padding: 4px 12px", css);
        Assert.Contains("min-width: 2.5rem", css);
    }

    [TestMethod]
    public void Stylesheet_has_the_collapsible_and_visually_hidden_classes_and_only_token_colors_in_them()
    {
        string css = ReadCss();
        foreach (string name in new[] { "gsp-collapsible-header", "gsp-collapsible-heading", "gsp-collapsible-toggle", "gsp-collapsible-summary", "gsp-collapsible-body", "gsp-visually-hidden" })
        {
            Assert.Contains("." + name, css, name);
        }
        int start = css.IndexOf("/* ---- More components", StringComparison.Ordinal);
        Assert.IsGreaterThan(0, start);
        string added = css[start..];
        Assert.DoesNotContain("--mud-", added);
        Assert.IsFalse(Regex.IsMatch(added, @"#[0-9a-fA-F]{3,8}\b|rgba?\("), "colors only through --gsp-* tokens");
    }
}
