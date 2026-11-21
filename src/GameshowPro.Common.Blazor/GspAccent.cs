namespace GameshowPro.Common.Blazor;

/// <summary>
/// One theme's accent colors, all <c>#rrggbb</c>.
/// </summary>
/// <param name="Fill">Background of an accent-filled control (primary button, selected item, badge).</param>
/// <param name="OnFill">Text and icons drawn on <paramref name="Fill"/>; must reach 4.5:1 on it.</param>
/// <param name="Text">The accent used as text or an outline on the page background and panels (links, text buttons,
/// focus rings, MudBlazor's <c>Primary</c>); must reach 4.5:1 on the background, panel and sunken colors. It may
/// equal <paramref name="Fill"/> when the fill is already dark (light theme) or light (dark theme) enough.</param>
public sealed partial record GspAccentColors(string Fill, string OnFill, string Text)
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColor();

    /// <summary>The validated <c>#rrggbb</c> fill; anything else throws, so the values are safe to write into CSS.</summary>
    public string Fill { get; } = Check(Fill, nameof(Fill));

    /// <summary>The validated <c>#rrggbb</c> color of text drawn on the fill.</summary>
    public string OnFill { get; } = Check(OnFill, nameof(OnFill));

    /// <summary>The validated <c>#rrggbb</c> accent-as-text color.</summary>
    public string Text { get; } = Check(Text, nameof(Text));

    private static string Check(string value, string name)
        => value is not null && HexColor().IsMatch(value) ? value.ToLowerInvariant() : throw new ArgumentException($"{name} must be a #rrggbb color, was '{value}'.", name);
}

/// <summary>
/// A product's accent: the one color that identifies it, per theme. The neutral surfaces, text and status colors are
/// the same for every product (<see cref="GspHouseTheme"/>); only this varies.
/// </summary>
/// <remarks>
/// <para>
/// Choosing values: start from the brand color as <see cref="GspAccentColors.Fill"/>. Pick <see cref="GspAccentColors.OnFill"/>
/// (near-black or white) so it reaches 4.5:1 on the fill. Then pick <see cref="GspAccentColors.Text"/>: if the fill already
/// reaches 4.5:1 on the page background, panel and sunken colors of that theme, reuse it; otherwise darken (light theme)
/// or lighten (dark theme) it, keeping its hue, until it does. A bright brand yellow, for example, is a fine fill in
/// both themes but needs a dark gold as light-theme text. <c>GspHouseTheme</c> uses <c>Text</c> for MudBlazor's
/// <c>Primary</c>, because MudBlazor draws <c>Primary</c> as text and outlines too; the contrast tests in this repository
/// (<c>HouseThemeContrastTests</c>) show how to guard the pairs.
/// </para>
/// </remarks>
/// <param name="Light">Accent in the light theme.</param>
/// <param name="Dark">Accent in the dark theme.</param>
public sealed record GspAccent(GspAccentColors Light, GspAccentColors Dark)
{
    /// <summary>Suede's indigo: <c>#3b5bdb</c> (white text) in the light theme, <c>#7d92f0</c> (near-black text) in the dark theme.</summary>
    public static GspAccent Indigo { get; } = new(
        new GspAccentColors("#3b5bdb", "#ffffff", "#3b5bdb"),
        new GspAccentColors("#7d92f0", "#13151a", "#7d92f0"));
}
