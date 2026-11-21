namespace GameshowPro.Common.Blazor.Test;

[TestClass]
public sealed class HouseIndexHtmlTests
{
    private static string Defaults(GspAccentColors value, string name)
        => $"--gsp-default-{name}: {value.Fill}; --gsp-default-{name}-ink: {value.OnFill}; --gsp-default-{name}-text: {value.Text};";

    private static string Host(GspAccent accent, GspAccent? secondary = null)
        => "<style>:root {" + Defaults(accent.Light, "accent") + (secondary is null ? "" : Defaults(secondary.Light, "accent-2"))
            + "} @media (prefers-color-scheme: dark) {:root {" + Defaults(accent.Dark, "accent")
            + (secondary is null ? "" : Defaults(secondary.Dark, "accent-2")) + "}}</style>";

    [TestMethod]
    public void MatchingPrimaryAndSecondaryPass()
        => Assert.AreEqual(0, GspIndexHtml.ValidateAccentDefaults(Host(GspAccent.Indigo, GspAccent.Indigo), GspAccent.Indigo, GspAccent.Indigo).Count);

    [TestMethod]
    public void PrimaryOnlyDoesNotRequireSecondary()
        => Assert.AreEqual(0, GspIndexHtml.ValidateAccentDefaults(Host(GspAccent.Indigo), GspAccent.Indigo).Count);

    [TestMethod]
    public void DriftNamesTheModeAndVariable()
    {
        string html = Host(GspAccent.Indigo).Replace(GspAccent.Indigo.Dark.Text, "#123456", StringComparison.Ordinal);
        IReadOnlyList<string> errors = GspIndexHtml.ValidateAccentDefaults(html, GspAccent.Indigo);
        Assert.IsTrue(errors.Any(error => error.Contains("dark --gsp-default-accent-text", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void CommentAndTextOutsideStyleCannotSatisfyMissingDefaults()
    {
        string declarations = Host(GspAccent.Indigo).Replace("<style>", "", StringComparison.Ordinal).Replace("</style>", "", StringComparison.Ordinal);
        Assert.AreEqual(6, GspIndexHtml.ValidateAccentDefaults("<style>/*" + declarations + "*/</style>", GspAccent.Indigo).Count);
        Assert.AreEqual(6, GspIndexHtml.ValidateAccentDefaults(Defaults(GspAccent.Indigo.Light, "accent"), GspAccent.Indigo).Count);
    }

    [TestMethod]
    public void LastDuplicateValueIsValidated()
    {
        string html = Host(GspAccent.Indigo).Replace(":root {", ":root {--gsp-default-accent: #000000;", StringComparison.Ordinal);
        Assert.AreEqual(0, GspIndexHtml.ValidateAccentDefaults(html, GspAccent.Indigo).Count);
        html = Host(GspAccent.Indigo).Replace("} @media", "--gsp-default-accent: #000000;} @media", StringComparison.Ordinal);
        Assert.IsTrue(GspIndexHtml.ValidateAccentDefaults(html, GspAccent.Indigo).Any(error => error.StartsWith("light --gsp-default-accent:", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void UppercaseHexAndWhitespaceAreAccepted()
    {
        string html = Host(GspAccent.Indigo).Replace("#3b5bdb", "#3B5BDB", StringComparison.Ordinal)
            .Replace("prefers-color-scheme: dark", "prefers-color-scheme :   dark", StringComparison.Ordinal);
        Assert.AreEqual(0, GspIndexHtml.ValidateAccentDefaults(html, GspAccent.Indigo).Count);
    }
}
