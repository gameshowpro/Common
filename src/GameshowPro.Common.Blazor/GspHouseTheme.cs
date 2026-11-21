namespace GameshowPro.Common.Blazor;

/// <summary>
/// <para>
/// The GameshowPro house style as a MudBlazor theme: Suede's quiet neutral surfaces, thin borders and modest rounding,
/// light and dark, with one accent per product (<see cref="GspAccent"/>). The C# palettes here are the single source of
/// truth for color: <c>gsp-house.css</c> reads the generated <c>--mud-palette-*</c> variables, and
/// <see cref="GspThemeProvider"/> writes the accent variables from the same <see cref="GspAccent"/>.
/// </para>
/// <para>
/// Tokens (light / dark): background <c>#f5f6f8</c> / <c>#13151a</c>, panel (MudBlazor <c>Surface</c>) <c>#ffffff</c> /
/// <c>#1b1e24</c>, sunken (<c>BackgroundGray</c>) <c>#f0f2f5</c> / <c>#15171c</c>, ink <c>#16191d</c> / <c>#e8eaed</c>,
/// muted <c>#667085</c> / <c>#98a2b3</c>, line <c>#e2e6ea</c> / <c>#2a2f38</c>, success <c>#257e36</c> / <c>#51cf66</c>,
/// warning <c>#8f6809</c> / <c>#f0b849</c>, error <c>#c92a2a</c> / <c>#ff6b6b</c>. The dark values are Suede's. In the
/// light theme the success and warning colors are darker than Suede's (<c>#2f9e44</c>, <c>#b8860b</c>, about 3:1 as
/// text) with the same hue and saturation, so that they reach WCAG AA (4.5:1) wherever they are drawn as text.
/// </para>
/// <para>
/// A product may give a second accent; it maps to MudBlazor's <c>Secondary</c> the same way. MudBlazor's <c>Primary</c> is the accent's <see cref="GspAccentColors.Text"/>, since MudBlazor draws it as text,
/// outlines and focus; <c>PrimaryContrastText</c> is whichever of white and the accent's
/// <see cref="GspAccentColors.OnFill"/> reads better on it. A filled <c>Color.Primary</c> MudBlazor control therefore
/// uses the accent text color as its fill; plain markup that wants the exact brand fill uses
/// <c>var(--gsp-accent)</c> and <c>var(--gsp-accent-ink)</c>.
/// </para>
/// </summary>
public static class GspHouseTheme
{
    private const string LightInk = "#16191d";
    private const string DarkInk = "#e8eaed";

    /// <summary>The house neutral that MudBlazor's <c>Secondary</c> is when a product has no second accent (light theme).</summary>
    public static GspAccentColors NeutralSecondaryLight { get; } = new("#667085", "#ffffff", "#667085");

    /// <summary>The house neutral that MudBlazor's <c>Secondary</c> is when a product has no second accent (dark theme).</summary>
    public static GspAccentColors NeutralSecondaryDark { get; } = new("#98a2b3", "#13151a", "#98a2b3");

    /// <summary>
    /// A new house theme with the given accent. Pass the same arguments to <see cref="GspThemeProvider"/>. The optional
    /// <paramref name="secondary"/> accent becomes MudBlazor's <c>Secondary</c> (by its <see cref="GspAccentColors.Text"/>);
    /// without it <c>Secondary</c> stays the house neutral.
    /// </summary>
    public static MudTheme Create(GspAccent accent, GspAccent? secondary = null)
    {
        ArgumentNullException.ThrowIfNull(accent);
        return new MudTheme
        {
            PaletteLight = CreateLight(accent.Light, secondary?.Light),
            PaletteDark = CreateDark(accent.Dark, secondary?.Dark),
            Typography = CreateTypography(),
            LayoutProperties = new LayoutProperties { DefaultBorderRadius = "7px" },
        };
    }

    /// <summary>The light palette for <paramref name="accent"/> and the optional <paramref name="secondary"/> accent.</summary>
    public static PaletteLight CreateLight(GspAccentColors accent, GspAccentColors? secondary = null)
    {
        ArgumentNullException.ThrowIfNull(accent);
        PaletteLight palette = new()
        {
            Background = "#f5f6f8",
            BackgroundGray = "#f0f2f5",
            Surface = "#ffffff",
            AppbarBackground = "#ffffff",
            AppbarText = LightInk,
            DrawerBackground = "#ffffff",
            DrawerText = LightInk,
            DrawerIcon = "#667085",
            TextPrimary = LightInk,
            TextSecondary = "#667085",
            TextDisabled = "rgba(22,25,29,0.38)",
            ActionDefault = "#667085",
            ActionDisabled = "rgba(22,25,29,0.26)",
            ActionDisabledBackground = "rgba(22,25,29,0.12)",
            LinesDefault = "#e2e6ea",
            LinesInputs = "#98a2b3",
            TableLines = "#e2e6ea",
            Divider = "#e2e6ea",
            DividerLight = "#eef0f3",
            Black = LightInk,
            Success = "#257e36",
            SuccessContrastText = "#ffffff",
            Warning = "#8f6809",
            WarningContrastText = "#ffffff",
            Error = "#c92a2a",
            ErrorContrastText = "#ffffff",
            Info = "#1c6fb8",
            InfoContrastText = "#ffffff",
            Tertiary = "#1c6fb8",
            TertiaryContrastText = "#ffffff",
            Dark = LightInk,
            DarkContrastText = "#ffffff",
        };
        ApplyAccents(palette, accent, secondary ?? NeutralSecondaryLight);
        ApplyShades(palette, darkenForText: true);
        return palette;
    }

    /// <summary>The dark palette for <paramref name="accent"/> and the optional <paramref name="secondary"/> accent.</summary>
    public static PaletteDark CreateDark(GspAccentColors accent, GspAccentColors? secondary = null)
    {
        ArgumentNullException.ThrowIfNull(accent);
        PaletteDark palette = new()
        {
            Background = "#13151a",
            BackgroundGray = "#15171c",
            Surface = "#1b1e24",
            AppbarBackground = "#1b1e24",
            AppbarText = DarkInk,
            DrawerBackground = "#1b1e24",
            DrawerText = DarkInk,
            DrawerIcon = "#98a2b3",
            TextPrimary = DarkInk,
            TextSecondary = "#98a2b3",
            TextDisabled = "rgba(232,234,237,0.38)",
            ActionDefault = "#98a2b3",
            ActionDisabled = "rgba(232,234,237,0.26)",
            ActionDisabledBackground = "rgba(232,234,237,0.12)",
            LinesDefault = "#2a2f38",
            LinesInputs = "#566074",
            TableLines = "#2a2f38",
            Divider = "#2a2f38",
            DividerLight = "#23272e",
            Black = "#0c0d10",
            Success = "#51cf66",
            SuccessContrastText = "#13151a",
            Warning = "#f0b849",
            WarningContrastText = "#13151a",
            Error = "#ff6b6b",
            ErrorContrastText = "#13151a",
            Info = "#74b4f0",
            InfoContrastText = "#13151a",
            Tertiary = "#74b4f0",
            TertiaryContrastText = "#13151a",
            Dark = "#2a2f38",
            DarkContrastText = DarkInk,
        };
        ApplyAccents(palette, accent, secondary ?? NeutralSecondaryDark);
        ApplyShades(palette, darkenForText: false);
        return palette;
    }

    private static void ApplyAccents(Palette palette, GspAccentColors primary, GspAccentColors secondary)
    {
        palette.Primary = primary.Text;
        palette.PrimaryContrastText = ContrastTextOnText(primary);
        palette.Secondary = secondary.Text;
        palette.SecondaryContrastText = ContrastTextOnText(secondary);
    }

    // MudBlazor fills Color.Primary / Color.Secondary controls with the accent's *text* color, so their text must read on that.
    private static string ContrastTextOnText(GspAccentColors accent)
        => GspContrast.Ratio(new MudColor("#ffffff"), new MudColor(accent.Text)) >= GspContrast.Ratio(new MudColor(accent.OnFill), new MudColor(accent.Text))
            ? "#ffffff"
            : accent.OnFill;

    /// <summary>
    /// Sets the Darken/Lighten variants MudBlazor uses for hover, pressed and alert text, which it would otherwise
    /// leave at its own defaults regardless of our colors. Light theme: Darken is darker (alert text on a faint tint of
    /// the color stays readable). Dark theme: Darken stays at the color itself (darkening a light color on a dark
    /// surface lowers its contrast), except Primary, whose Darken is the fill of hovered filled buttons.
    /// </summary>
    private static void ApplyShades(Palette palette, bool darkenForText)
    {
        static string Darker(MudColor c) => c.ColorRgbDarken().ToString(MudColorOutputFormats.Hex);
        static string Lighter(MudColor c) => c.ColorRgbLighten().ToString(MudColorOutputFormats.Hex);
        string TextShade(MudColor c) => darkenForText ? Darker(c) : c.ToString(MudColorOutputFormats.Hex);

        palette.PrimaryDarken = Darker(palette.Primary);
        palette.PrimaryLighten = Lighter(palette.Primary);
        palette.SecondaryDarken = TextShade(palette.Secondary);
        palette.SecondaryLighten = Lighter(palette.Secondary);
        palette.TertiaryDarken = TextShade(palette.Tertiary);
        palette.TertiaryLighten = Lighter(palette.Tertiary);
        palette.InfoDarken = TextShade(palette.Info);
        palette.InfoLighten = Lighter(palette.Info);
        palette.SuccessDarken = TextShade(palette.Success);
        palette.SuccessLighten = Lighter(palette.Success);
        palette.WarningDarken = TextShade(palette.Warning);
        palette.WarningLighten = Lighter(palette.Warning);
        palette.ErrorDarken = TextShade(palette.Error);
        palette.ErrorLighten = Lighter(palette.Error);
    }

    private static readonly string[] s_sans = ["Plus Jakarta Sans", "system-ui", "-apple-system", "Segoe UI", "sans-serif"];

    /// <summary>
    /// Plus Jakarta Sans: body 15px / 1.55, 400; subtitles and buttons 600 (buttons sentence case); headings 700 with
    /// tight tracking; overline 11px uppercase +0.05em. Sizes are rem-based (root stays 16px). Numbers and identifiers
    /// switch to the mono family with the <c>.gsp-data</c> CSS class.
    /// </summary>
    private static Typography CreateTypography() => new()
    {
        Default = Type<DefaultTypography>(0.9375, 400, 1.55, "0.01em"),
        H1 = Type<H1Typography>(2.5, 700, 1.2, "-0.02em"),
        H2 = Type<H2Typography>(2, 700, 1.2, "-0.02em"),
        H3 = Type<H3Typography>(1.625, 700, 1.25, "-0.02em"),
        H4 = Type<H4Typography>(1.375, 700, 1.3, "-0.01em"),
        H5 = Type<H5Typography>(1.25, 700, 1.3, "-0.01em"),
        H6 = Type<H6Typography>(1.0625, 700, 1.3, "-0.005em"),
        Subtitle1 = Type<Subtitle1Typography>(0.9375, 600, 1.4, "0.01em"),
        Subtitle2 = Type<Subtitle2Typography>(0.8125, 600, 1.4, "0.01em"),
        Body1 = Type<Body1Typography>(0.9375, 400, 1.55, "0.01em"),
        Body2 = Type<Body2Typography>(0.875, 400, 1.5, "0.01em"),
        Button = Type<ButtonTypography>(0.8125, 600, 1.75, "0.01em", "none"),
        Caption = Type<CaptionTypography>(0.75, 400, 1.4, "0.01em"),
        Overline = Type<OverlineTypography>(0.6875, 600, 1.5, "0.05em", "uppercase"),
    };

    private static T Type<T>(double rem, int weight, double lineHeight, string letterSpacing, string textTransform = "none")
        where T : BaseTypography, new()
        => new()
        {
            FontFamily = s_sans,
            FontSize = $"{rem.ToString("0.####", CultureInfo.InvariantCulture)}rem",
            FontWeight = weight.ToString(CultureInfo.InvariantCulture),
            LineHeight = lineHeight.ToString("0.##", CultureInfo.InvariantCulture),
            LetterSpacing = letterSpacing,
            TextTransform = textTransform,
        };
}
