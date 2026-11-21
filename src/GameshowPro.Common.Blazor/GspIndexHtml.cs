namespace GameshowPro.Common.Blazor;

/// <summary>Checks a host page's pre-start accent declarations against its registered accents, without a test-framework dependency.</summary>
/// <remarks>
/// Expects light defaults in a :root rule and dark defaults in a :root rule inside
/// @media (prefers-color-scheme: dark), in inline style elements. This validates that convention,
/// not arbitrary CSS cascade semantics. Pass the returned errors to your test framework's empty assertion.
/// </remarks>
public static partial class GspIndexHtml
{
    /// <summary>Returns missing or mismatched pre-start accent declarations; an empty result means both modes agree.</summary>
    public static IReadOnlyList<string> ValidateAccentDefaults(string html, GspAccent accent, GspAccent? secondaryAccent = null)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(accent);
        string css = string.Join("\n", Styles().Matches(html).Select(match => match.Groups["css"].Value));
        css = Comments().Replace(css, "");
        Match dark = DarkDefaults().Match(css);
        Match light = RootDefaults().Match(DarkDefaults().Replace(css, ""));
        List<string> errors = [];
        Check("light", light.Success ? light.Groups["css"].Value : "", accent.Light, secondaryAccent?.Light, errors);
        Check("dark", dark.Success ? dark.Groups["css"].Value : "", accent.Dark, secondaryAccent?.Dark, errors);
        return errors;
    }

    private static void Check(string mode, string css, GspAccentColors accent, GspAccentColors? secondary, List<string> errors)
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        foreach (Match declaration in Declarations().Matches(css))
        {
            values[declaration.Groups["name"].Value] = declaration.Groups["value"].Value.Trim();
        }
        Compare("accent", accent.Fill);
        Compare("accent-ink", accent.OnFill);
        Compare("accent-text", accent.Text);
        if (secondary is not null)
        {
            Compare("accent-2", secondary.Fill);
            Compare("accent-2-ink", secondary.OnFill);
            Compare("accent-2-text", secondary.Text);
        }

        void Compare(string name, string expected)
        {
            string variable = "--gsp-default-" + name;
            values.TryGetValue(variable, out string? actual);
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"{mode} {variable}: expected {expected}, found {actual ?? "no declaration"}.");
            }
        }
    }

    [GeneratedRegex(@"<style\b[^>]*>(?<css>[\s\S]*?)</style\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex Styles();
    [GeneratedRegex(@"/\*[\s\S]*?\*/")]
    private static partial Regex Comments();
    [GeneratedRegex(@"@media\s*\(\s*prefers-color-scheme\s*:\s*dark\s*\)\s*\{\s*:root\s*\{(?<css>[^}]*)\}\s*\}", RegexOptions.IgnoreCase)]
    private static partial Regex DarkDefaults();
    [GeneratedRegex(@":root\s*\{(?<css>[^}]*)\}", RegexOptions.IgnoreCase)]
    private static partial Regex RootDefaults();
    [GeneratedRegex(@"(?<name>--gsp-default-[a-z0-9-]+)\s*:\s*(?<value>[^;{}]+)\s*;")]
    private static partial Regex Declarations();
}
