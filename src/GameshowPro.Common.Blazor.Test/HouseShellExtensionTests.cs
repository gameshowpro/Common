namespace GameshowPro.Common.Blazor.Test;

/// <summary>The shell can leave page layout to its host while keeping the house providers and overlay.</summary>
[TestClass]
public sealed class HouseShellExtensionTests
{
    private static BunitContext CreateContext(bool footer = false)
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddGspHouseStyle(options => options.ShowFooter = footer);
        return context;
    }

    [TestMethod]
    public async Task Frame_mode_omits_header_and_content_column_but_keeps_house_services()
    {
        await using BunitContext context = CreateContext(footer: true);
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>(parameters => parameters
            .Add(component => component.ShowHeader, false)
            .Add(component => component.WrapContent, false)
            .AddChildContent("<section id=\"page\">content</section>"));

        Assert.IsEmpty(shell.FindAll("header.gsp-header"));
        Assert.IsEmpty(shell.FindAll("main.gsp-main"));
        Assert.AreEqual("content", shell.Find("#page").TextContent);
        Assert.IsNotNull(shell.Find("footer.gsp-footer"));
        Assert.IsNotNull(shell.Find("dialog.gsp-dialog"));
        Assert.IsNotNull(shell.Find("button.gsp-about-button"));

        context.Services.GetRequiredService<GspRuntimeState>().Connection = RemoteServiceStates.Warning;
        shell.WaitForAssertion(() => Assert.IsNotNull(shell.Find(".gsp-connection-card")));
    }

    [TestMethod]
    public async Task Leading_content_sits_between_brand_and_existing_header_actions()
    {
        await using BunitContext context = CreateContext();
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>(parameters => parameters
            .Add(component => component.HeaderLeadingContent, (RenderFragment)(builder => builder.AddMarkupContent(0, "<button id=\"back\">Back</button>")))
            .Add(component => component.HeaderContent, (RenderFragment)(builder => builder.AddMarkupContent(0, "<button id=\"action\">Action</button>")))
            .AddChildContent("<p id=\"page\">content</p>"));

        IElement header = shell.Find("header.gsp-header");
        string html = header.InnerHtml;
        Assert.IsTrue(html.IndexOf("gsp-brand", StringComparison.Ordinal) < html.IndexOf("gsp-header-leading", StringComparison.Ordinal));
        Assert.IsTrue(html.IndexOf("gsp-header-leading", StringComparison.Ordinal) < html.IndexOf("gsp-header-actions", StringComparison.Ordinal));
        Assert.AreEqual("Back", header.QuerySelector(".gsp-header-leading #back")?.TextContent);
        Assert.AreEqual("Action", header.QuerySelector(".gsp-header-actions #action")?.TextContent);
        Assert.AreEqual("content", shell.Find("main.gsp-main #page").TextContent);
    }

    [TestMethod]
    public async Task Runtime_overlay_text_updates_and_null_restores_the_default_title()
    {
        await using BunitContext context = CreateContext();
        GspRuntimeState state = context.Services.GetRequiredService<GspRuntimeState>();
        state.Connection = RemoteServiceStates.Warning;
        IRenderedComponent<GspAppShell> shell = context.Render<GspAppShell>();
        string defaultTitle = shell.Find(".gsp-connection-title").TextContent;
        Assert.IsNotEmpty(defaultTitle);
        Assert.IsEmpty(shell.FindAll(".gsp-connection-detail"));

        state.ConnectionTitle = "Service restarting";
        state.ConnectionDetail = "Retrying connection (attempt 2)";
        shell.WaitForAssertion(() => Assert.AreEqual("Service restarting", shell.Find(".gsp-connection-title").TextContent));
        Assert.AreEqual("Retrying connection (attempt 2)", shell.Find(".gsp-connection-detail").TextContent);

        state.ConnectionTitle = null;
        state.ConnectionDetail = null;
        shell.WaitForAssertion(() => Assert.AreEqual(defaultTitle, shell.Find(".gsp-connection-title").TextContent));
        Assert.IsEmpty(shell.FindAll(".gsp-connection-detail"));
    }
}
