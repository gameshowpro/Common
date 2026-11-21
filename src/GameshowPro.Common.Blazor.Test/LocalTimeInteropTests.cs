namespace GameshowPro.Common.Blazor.Test;

/// <summary>
/// Failure handling around the browser time zone lookup: the header and About dialog must not retry a failing interop call on
/// every render, and <see cref="GspLocalTimeFormatter"/> must dispose safely whatever the interop does.
/// </summary>
[TestClass]
public class LocalTimeInteropTests
{
    private static readonly DateTimeOffset s_instant = new(2026, 11, 1, 8, 30, 0, TimeSpan.Zero);

    private static BunitContext CreateContext(ScriptedJs js)
    {
        BunitContext context = new();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddGspHouseStyle();
        context.Services.AddSingleton(sp => GspAppInfo.Resolve(sp.GetRequiredService<GspHouseOptions>(), typeof(LocalTimeInteropTests).Assembly));
        context.Services.AddSingleton<IJSRuntime>(js);
        return context;
    }

    [TestMethod]
    public async Task Header_attempts_the_build_time_lookup_once_when_interop_fails()
    {
        ScriptedJs js = new() { FailImport = true };
        await using BunitContext context = CreateContext(js);
        IRenderedComponent<GspAppHeader> header = context.Render<GspAppHeader>();
        Assert.AreEqual(1, js.ImportCount);

        for (int i = 0; i < 5; i++)
        {
            header.Render(_ => { });
        }

        Assert.AreEqual(1, js.ImportCount, "the failed lookup is not retried on later renders");
        Assert.AreEqual("Build time unavailable", header.Find(".gsp-version").GetAttribute("title"));
    }

    [TestMethod]
    public async Task About_dialog_attempts_the_build_time_lookup_once_when_interop_fails()
    {
        ScriptedJs js = new() { FailImport = true };
        await using BunitContext context = CreateContext(js);
        IRenderedComponent<GspAboutDialog> dialog = context.Render<GspAboutDialog>();
        Assert.AreEqual(1, js.ImportCount);

        for (int i = 0; i < 5; i++)
        {
            dialog.Render(_ => { });
        }

        Assert.AreEqual(1, js.ImportCount, "the failed lookup is not retried on later renders");
        Assert.AreEqual("Build time unavailable", dialog.FindAll(".gsp-kv td")[1].TextContent);
    }

    [TestMethod]
    public async Task Header_and_about_dialog_still_show_the_build_time_when_interop_succeeds()
    {
        ScriptedJs js = new();
        await using BunitContext context = CreateContext(js);
        IRenderedComponent<GspAppHeader> header = context.Render<GspAppHeader>();
        IRenderedComponent<GspAboutDialog> dialog = context.Render<GspAboutDialog>();

        header.WaitForAssertion(() => Assert.StartsWith("Built 20", header.Find(".gsp-version").GetAttribute("title") ?? ""));
        dialog.WaitForAssertion(() => Assert.Contains("UTC+00:00 (Etc/UTC)", dialog.FindAll(".gsp-kv td")[1].TextContent));
    }

    [TestMethod]
    public async Task Disposing_the_formatter_twice_does_not_throw()
    {
        ScriptedJs js = new();
        GspLocalTimeFormatter formatter = new(js);
        await formatter.PrimeAsync([s_instant]);

        await formatter.DisposeAsync();
        await formatter.DisposeAsync();

        Assert.AreEqual(1, js.ModuleDisposeCount, "the module is released once");
    }

    [TestMethod]
    public async Task Priming_after_disposal_makes_no_interop_call_and_keeps_the_cache()
    {
        ScriptedJs js = new();
        GspLocalTimeFormatter formatter = new(js);
        await formatter.PrimeAsync([s_instant]);
        int imports = js.ImportCount;
        int describes = js.DescribeCount;
        await formatter.DisposeAsync();

        await formatter.PrimeAsync([s_instant.AddHours(1)]);

        Assert.AreEqual(imports, js.ImportCount);
        Assert.AreEqual(describes, js.DescribeCount);
        Assert.IsNotNull(formatter.FormatCached(s_instant), "what was cached stays available");
        Assert.IsNull(formatter.FormatCached(s_instant.AddHours(1)));
    }

    [TestMethod]
    public async Task A_module_that_fails_to_dispose_does_not_make_the_formatter_throw()
    {
        ScriptedJs js = new() { ModuleDisposeException = new InvalidOperationException("module failure") };
        GspLocalTimeFormatter formatter = new(js);
        await formatter.PrimeAsync([s_instant]);

        await formatter.DisposeAsync();

        Assert.AreEqual(1, js.ModuleDisposeCount);
    }

    [TestMethod]
    public async Task Disposing_while_a_prime_is_awaiting_interop_lets_the_prime_finish()
    {
        ScriptedJs js = new() { DescribeGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously) };
        GspLocalTimeFormatter formatter = new(js);
        Task prime = formatter.PrimeAsync([s_instant]);
        Assert.AreEqual(1, js.DescribeCount, "the prime is waiting inside interop");

        Task dispose = formatter.DisposeAsync().AsTask();
        await Task.Delay(50, TestContext.CancellationToken);
        Assert.IsFalse(dispose.IsCompleted, "disposal waits for the prime that holds the gate");

        js.DescribeGate.SetResult();
        await prime;
        await dispose;

        Assert.AreEqual(1, js.ModuleDisposeCount);
        Assert.IsNotNull(formatter.FormatCached(s_instant));
    }

    public TestContext TestContext { get; set; } = null!;

    // This in-memory test fake returns data directly; no browser interop occurs here.
#pragma warning disable BL0016
    private sealed class ScriptedJs : IJSRuntime, IJSObjectReference
    {
        public bool FailImport { get; init; }

        public TaskCompletionSource? DescribeGate { get; init; }

        public Exception? ModuleDisposeException { get; init; }

        public int ImportCount { get; private set; }

        public int DescribeCount { get; private set; }

        public int ModuleDisposeCount { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => Respond<TValue>(identifier, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => Respond<TValue>(identifier, args);

        private async ValueTask<TValue> Respond<TValue>(string identifier, object?[]? args)
        {
            switch (identifier)
            {
                case "import":
                    ImportCount++;
                    if (FailImport)
                    {
                        throw new JSException("interop is unavailable");
                    }
                    return (TValue)(object)this;
                case "describe":
                    DescribeCount++;
                    if (DescribeGate is not null)
                    {
                        await DescribeGate.Task;
                    }
                    string[] reply = ["Etc/UTC", .. Enumerable.Repeat("0", ((string[])args![0]!).Length)];
                    return (TValue)(object)reply;
                default:
                    throw new InvalidOperationException(identifier);
            }
        }

        public ValueTask DisposeAsync()
        {
            ModuleDisposeCount++;
            return ModuleDisposeException is { } exception ? ValueTask.FromException(exception) : ValueTask.CompletedTask;
        }
    }
#pragma warning restore BL0016
}
