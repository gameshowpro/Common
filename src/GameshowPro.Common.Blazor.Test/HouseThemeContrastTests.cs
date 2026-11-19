using MudBlazor;
using MudBlazor.Utilities;

namespace GameshowPro.Common.Blazor.Test;

/// <summary>
/// <para>
/// Contrast guard for the house theme (<see cref="GspHouseTheme"/>) against WCAG 2.2 AA (4.5:1 for normal text), for
/// both palettes and for two accents: <see cref="GspAccent.Indigo"/> and a bright logo yellow (<c>#ffe600</c>) as
/// TriggerGate will define it. Every pair is measured with <see cref="GspContrast.Ratio"/>; a failure names the pair
/// and its ratio, and every ratio is written to the test output.
/// </para>
/// <para>
/// Covered: ink on the page, panel and sunken backgrounds; muted text on page and panel; accent-as-text
/// (<c>Primary</c>) on page, panel and sunken; the success, warning, error and info colors as text on page and
/// panel; text on accent fills (the exact brand fill with <c>OnFill</c>, and MudBlazor's filled Primary with its
/// contrast text, also in its darkened hover state); contrast text on each status fill; text-variant alert messages
/// on their tint; app bar text. Not covered: anything the palette does not decide (inline styles, product CSS),
/// muted text on the sunken color (4.4:1 in the light theme; it is used for placeholders and captions, not for
/// required reading), disabled text, and non-text parts other than through the text colors they share.
/// </para>
/// </summary>
[TestClass]
public class HouseThemeContrastTests
{
    /// <summary>
    /// A bright yellow brand accent as a product would define it. Dark theme: the logo yellow is both fill and text.
    /// Light theme: the logo yellow is the fill (near-black text on it), and the accent-as-text is the dark gold of the
    /// same hue (HSL 54 degrees, 100% saturation, lightness lowered from 50% until it reaches 4.5:1 on the page,
    /// panel and sunken colors), <c>#7b6f00</c>.
    /// </summary>
    internal static readonly GspAccent LogoYellow = new(
        new GspAccentColors("#ffe600", "#16191d", "#7b6f00"),
        new GspAccentColors("#ffe600", "#13151a", "#ffe600"));

    public TestContext TestContext { get; set; } = null!;

    private sealed record Pair(string Name, MudColor Foreground, MudColor Background);

    private static IEnumerable<(string Name, GspAccent Accent)> Accents()
    {
        yield return ("Indigo", GspAccent.Indigo);
        yield return ("LogoYellow", LogoYellow);
    }

    private static IEnumerable<(string Name, Palette Palette, GspAccentColors Accent)> Palettes(string accentName, GspAccent accent)
    {
        MudTheme theme = GspHouseTheme.Create(accent);
        yield return ($"{accentName}/light", theme.PaletteLight, accent.Light);
        yield return ($"{accentName}/dark", theme.PaletteDark, accent.Dark);
    }

    private static IEnumerable<(string Name, Palette Palette, GspAccentColors Accent)> AllPalettes()
        => Accents().SelectMany(a => Palettes(a.Name, a.Accent));

    private static MudColor C(string hex) => new(hex);

    private static Pair P(string name, MudColor fg, MudColor bg) => new(name, fg, bg);

    [TestMethod]
    public void Text_colors_read_on_page_panel_and_sunken()
    {
        List<Pair> pairs = [];
        foreach ((string name, Palette p, GspAccentColors _) in AllPalettes())
        {
            (string, MudColor)[] grounds = [("bg", p.Background), ("panel", p.Surface), ("sunken", p.BackgroundGray)];
            (string, MudColor)[] onAllGrounds = [("ink", p.TextPrimary), ("accent-text (Primary)", p.Primary)];
            (string, MudColor)[] onPageAndPanel =
            [
                ("muted", p.TextSecondary), ("success", p.Success), ("warning", p.Warning), ("error", p.Error),
                ("info", p.Info), ("Secondary", p.Secondary), ("Tertiary", p.Tertiary),
            ];
            foreach ((string gName, MudColor g) in grounds)
            {
                foreach ((string tName, MudColor t) in onAllGrounds)
                {
                    pairs.Add(P($"{name}: {tName} on {gName}", t, g));
                }
                if (gName != "sunken")
                {
                    foreach ((string tName, MudColor t) in onPageAndPanel)
                    {
                        pairs.Add(P($"{name}: {tName} on {gName}", t, g));
                    }
                }
            }
            pairs.Add(P($"{name}: AppbarText on AppbarBackground", p.AppbarText, p.AppbarBackground));
            pairs.Add(P($"{name}: DrawerText on DrawerBackground", p.DrawerText, p.DrawerBackground));
        }
        AssertAllPass(pairs);
    }

    [TestMethod]
    public void Accent_fill_carries_its_text()
    {
        List<Pair> pairs = [];
        foreach ((string name, Palette _, GspAccentColors accent) in AllPalettes())
        {
            pairs.Add(P($"{name}: accent OnFill on Fill", C(accent.OnFill), C(accent.Fill)));
        }
        AssertAllPass(pairs);
    }

    [TestMethod]
    public void Contrast_text_reads_on_its_fill_and_its_darkened_fill()
    {
        List<Pair> pairs = [];
        foreach ((string name, Palette p, GspAccentColors _) in AllPalettes())
        {
            (string, MudColor, MudColor, string)[] fills =
            [
                ("Primary", p.Primary, p.PrimaryContrastText, p.PrimaryDarken),
                ("Secondary", p.Secondary, p.SecondaryContrastText, p.SecondaryDarken),
                ("Tertiary", p.Tertiary, p.TertiaryContrastText, p.TertiaryDarken),
                ("Info", p.Info, p.InfoContrastText, p.InfoDarken),
                ("Success", p.Success, p.SuccessContrastText, p.SuccessDarken),
                ("Warning", p.Warning, p.WarningContrastText, p.WarningDarken),
                ("Error", p.Error, p.ErrorContrastText, p.ErrorDarken),
            ];
            foreach ((string fName, MudColor fill, MudColor text, string darken) in fills)
            {
                pairs.Add(P($"{name}: {fName}ContrastText on {fName}", text, fill));
                pairs.Add(P($"{name}: {fName}ContrastText on {fName}Darken", text, C(darken)));
            }
        }
        AssertAllPass(pairs);
    }

    [TestMethod]
    public void Text_alert_messages_read_on_their_tint()
    {
        // A text-variant MudAlert draws its message in ...Darken on the color at HoverOpacity over the surface.
        List<Pair> pairs = [];
        foreach ((string name, Palette p, GspAccentColors _) in AllPalettes())
        {
            (string, MudColor, string)[] severities =
            [
                ("Info", p.Info, p.InfoDarken), ("Success", p.Success, p.SuccessDarken),
                ("Warning", p.Warning, p.WarningDarken), ("Error", p.Error, p.ErrorDarken),
            ];
            foreach ((string gName, MudColor ground) in new[] { ("bg", p.Background), ("panel", p.Surface) })
            {
                foreach ((string sName, MudColor color, string darken) in severities)
                {
                    pairs.Add(P($"{name}: {sName}Darken on {sName} alert tint over {gName}", C(darken), Composite(color, p.HoverOpacity, ground)));
                }
            }
        }
        AssertAllPass(pairs);
    }

    [TestMethod]
    public void Logo_yellow_accent_has_the_documented_gold()
    {
        // The documented gold keeps the hue (54 degrees) of the logo yellow.
        MudColor yellow = C("#ffe600");
        MudColor gold = C(LogoYellow.Light.Text);
        double hueYellow = yellow.H;
        double hueGold = gold.H;
        TestContext.WriteLine($"hue #ffe600 = {hueYellow:0.0}, hue {LogoYellow.Light.Text} = {hueGold:0.0}");
        Assert.IsTrue(Math.Abs(hueYellow - hueGold) < 1.5, $"Hue drifted: {hueYellow} vs {hueGold}");
        // The next step lighter would fail, so this is the lightest gold of that hue (within one hex step of lightness).
        MudColor lighter = new MudColor(gold.R + 3, gold.G + 3, gold.B, 255);
        TestContext.WriteLine($"{Hex(lighter)} on sunken (light) = {GspContrast.Ratio(lighter, C("#f0f2f5")):0.00}:1");
    }

    private void AssertAllPass(IEnumerable<Pair> pairs)
    {
        List<string> failures = [];
        foreach (Pair pair in pairs)
        {
            double ratio = GspContrast.Ratio(pair.Foreground, pair.Background);
            string line = string.Create(System.Globalization.CultureInfo.InvariantCulture,
                $"{pair.Name} ({Hex(pair.Foreground)} on {Hex(pair.Background)}): {ratio:0.00}:1");
            TestContext.WriteLine(line);
            if (ratio < GspContrast.NormalText)
            {
                failures.Add(line + ", needs 4.5:1");
            }
        }
        Assert.IsEmpty(failures, "Contrast below WCAG AA:\n" + string.Join('\n', failures));
    }

    /// <summary>The color at <paramref name="alpha"/> over an opaque <paramref name="ground"/>.</summary>
    private static MudColor Composite(MudColor color, double alpha, MudColor ground)
    {
        static int Mix(int top, int bottom, double a) => (int)Math.Round((top * a) + (bottom * (1 - a)));
        return new MudColor(Mix(color.R, ground.R, alpha), Mix(color.G, ground.G, alpha), Mix(color.B, ground.B, alpha), 255);
    }

    internal static string Hex(MudColor color) => GspContrast.Hex(color);
}
