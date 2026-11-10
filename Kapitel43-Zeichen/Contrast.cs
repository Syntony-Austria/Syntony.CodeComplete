using System.Globalization;

namespace Library.Lending;

// Contrast ratio as defined in WCAG 2.2 for the sRGB colour space.
public static class Contrast
{
    public static double Ratio(string foreground, string background)
    {
        double a = Luminance(foreground);
        double b = Luminance(background);
        double lighter = Math.Max(a, b);
        double darker = Math.Min(a, b);
        return (lighter + 0.05) / (darker + 0.05);
    }

    // "#RRGGBB" -> relative luminance between 0 (black) and 1 (white).
    public static double Luminance(string hex) =>
        0.2126 * Channel(hex, 1) + 0.7152 * Channel(hex, 3)
        + 0.0722 * Channel(hex, 5);

    private static double Channel(string hex, int start)
    {
        double s = int.Parse(hex.AsSpan(start, 2), NumberStyles.HexNumber,
            CultureInfo.InvariantCulture) / 255.0;
        return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
    }
}

public sealed record ColourPair(string Name, string Text, string Background,
    double Minimum);

public static class LibraryPalette
{
    public const double NormalText = 4.5; // WCAG 2.2, SC 1.4.3
    public const double LargeText = 3.0;

    public static IReadOnlyList<ColourPair> Pairs { get; } =
    [
        new("body on paper", "#2B2B2B", "#FFFFFF", NormalText),
        new("hint on paper", "#666666", "#FFFFFF", NormalText),
        new("accent on paper", "#8A5A2B", "#FFFFFF", NormalText),
        new("overdue on tint", "#9E3B2F", "#F4ECE3", NormalText),
        new("body on dark", "#EDEDED", "#1E1E1E", NormalText),
        new("heading on dark", "#D9C2A9", "#1E1E1E", LargeText),
    ];

    public static IEnumerable<string> Failures(IEnumerable<ColourPair> pairs) =>
        pairs.Select(p => (p, ratio: Contrast.Ratio(p.Text, p.Background)))
            .Where(x => x.ratio < x.p.Minimum)
            .Select(x => string.Create(CultureInfo.InvariantCulture,
                $"{x.p.Name}: {x.ratio:0.00}:1, needs {x.p.Minimum:0.0}:1"));
}
