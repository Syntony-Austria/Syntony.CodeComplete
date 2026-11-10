using System.Globalization;
using System.Resources;

namespace Library.Lending;

public sealed record OverdueLine(string MemberId, int DaysLate,
    DateOnly DueDate, int FeeInCents);

// Text from the resources in the UI language, numbers and dates in the
// format culture. Two different settings, on purpose.
public static class OverdueScreen
{
    private static readonly ResourceManager Strings =
        new("Library.Lending.Resources.Strings", typeof(OverdueScreen).Assembly);

    public static string Text(string key, CultureInfo ui) =>
        Strings.GetString(key, ui)
        ?? throw new MissingManifestResourceException(key);

    public static string Render(IReadOnlyList<OverdueLine> lines,
        CultureInfo ui, CultureInfo format)
    {
        List<string> rows = [Text("Overdue.Title", ui)];
        foreach (OverdueLine l in lines)
        {
            rows.Add(string.Format(format, Text("Overdue.Line", ui),
                l.MemberId, l.DaysLate, l.DueDate, l.FeeInCents / 100m));
        }

        decimal total = lines.Sum(l => l.FeeInCents) / 100m;
        rows.Add(string.Format(format, Text("Overdue.Total", ui), total));
        return string.Join('\n', rows) + '\n';
    }
}
