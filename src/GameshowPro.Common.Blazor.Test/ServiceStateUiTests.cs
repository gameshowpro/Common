namespace GameshowPro.Common.Blazor.Test;

/// <summary>
/// The standardized ServiceState tree component: recursive rendering, the day-one Progress
/// semantics (null = busy/indeterminate unless connected; 0–1 = determinate with percentage;
/// >= 1 = steady), and self-subscription so state changes render without parent involvement.
/// </summary>
[TestClass]
public class ServiceStateUiTests
{
    private static BunitContext CreateContext()
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMudServices();
        return context;
    }

    private static ServiceState Node(string key, RemoteServiceStates state, string? detail = null, double? progress = null, params ServiceState[] children)
        => new(key, key, state, detail, progress, children.ToDictionary(c => c.Key, c => c));

    [TestMethod]
    public async Task RendersTreeWithStatesAndDetails()
    {
        await using BunitContext context = CreateContext();
        ServiceState root = Node("service", RemoteServiceStates.Warning, "degraded", 1,
            Node("engine", RemoteServiceStates.Connected, "v1.2.3", 1),
            Node("project", RemoteServiceStates.Warning, "No project loaded", 1));

        IRenderedComponent<ServiceStateUi> cut = context.Render<ServiceStateUi>(parameters => parameters.Add(p => p.DataContext, root));

        Assert.Contains("engine", cut.Markup);
        Assert.Contains("v1.2.3", cut.Markup);
        Assert.Contains("No project loaded", cut.Markup);
        // All nodes steady (progress 1): no activity indicators at all
        Assert.DoesNotContain("gsp-progress-", cut.Markup);
    }

    [TestMethod]
    public async Task ProgressSemanticsRenderCorrectIndicators()
    {
        await using BunitContext context = CreateContext();
        ServiceState root = Node("service", RemoteServiceStates.Warning, null, 1,
            Node("busy", RemoteServiceStates.Disconnected, "Connecting", null),
            Node("steady", RemoteServiceStates.Connected, "OK", null),
            Node("loading", RemoteServiceStates.Warning, "Caching", 0.4));

        IRenderedComponent<ServiceStateUi> cut = context.Render<ServiceStateUi>(parameters => parameters.Add(p => p.DataContext, root));

        // null + not connected -> indeterminate; null + connected -> nothing; fraction -> determinate + percent
        Assert.AreEqual(1, CountOf(cut.Markup, "gsp-progress-indeterminate"));
        Assert.AreEqual(1, CountOf(cut.Markup, "gsp-progress-determinate"));
        Assert.Contains("40", cut.Markup, "determinate node should show its percentage");
    }

    [TestMethod]
    public async Task UpdatesWhenStateChangesWithoutParentRerender()
    {
        await using BunitContext context = CreateContext();
        ServiceState root = Node("service", RemoteServiceStates.Disconnected, "starting", null);

        IRenderedComponent<ServiceStateUi> cut = context.Render<ServiceStateUi>(parameters => parameters.Add(p => p.DataContext, root));
        Assert.Contains("starting", cut.Markup);

        root.SetAll(RemoteServiceStates.Connected, "running", 1);

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("running", cut.Markup);
            Assert.DoesNotContain("gsp-progress-", cut.Markup);
        });
    }

    [TestMethod]
    public async Task OverlayDerivesVisibilityFromAggregateState()
    {
        await using BunitContext context = CreateContext();
        ServiceState root = Node("service", RemoteServiceStates.Disconnected, "down", null);

        IRenderedComponent<ServiceConnectionOverlay> cut = context.Render<ServiceConnectionOverlay>(parameters => parameters
            .Add(p => p.DataContext, root)
            .Add(p => p.Title, "Disconnected from Test service")
            .Add(p => p.Detail, "attempt 3"));

        Assert.Contains("Disconnected from Test service", cut.Markup);
        Assert.Contains("attempt 3", cut.Markup);
        Assert.Contains("gsp-connection-overlay", cut.Markup);
    }

    private static int CountOf(string haystack, string needle)
    {
        int count = 0;
        int index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }
}
