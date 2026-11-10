using System.Globalization;
using System.Text.RegularExpressions;

namespace Library.Lending.Tests;

// Checks the decision records in docs/adr and the ADR numbers in src.
internal static partial class AdrGate
{
    private static readonly string[] s_sections =
        ["## Status", "## Context", "## Decision", "## Consequences"];

    private static readonly string[] s_statuses =
        ["Proposed", "Accepted", "Deprecated", "Superseded"];

    private static readonly string[] s_fields =
        ["- Owner:", "- Reversal cost:", "- Review by:"];

    public static List<string> Problems(string root)
    {
        List<string> problems = [];
        foreach (string path in AdrFiles(root))
        {
            string name = Path.GetFileName(path);
            string[] lines = File.ReadAllLines(path);
            string title = $"# ADR-{name[..4]}: ";
            if (!Starts(lines[0], title))
            {
                problems.Add($"{name}: title must start with '{title}'");
            }

            foreach (string required in s_sections.Concat(s_fields))
            {
                if (!lines.Any(l => Starts(l, required)))
                {
                    problems.Add($"{name}: missing '{required}'");
                }
            }

            string status = LineAfter(lines, "## Status");
            if (!s_statuses.Any(s => Starts(status, s)))
            {
                problems.Add($"{name}: unknown status '{status}'");
            }
        }

        return problems;
    }

    public static List<string> MissingReferences(string root)
    {
        HashSet<string> known = AdrFiles(root)
            .Select(p => "ADR-" + Path.GetFileName(p)[..4])
            .ToHashSet(StringComparer.Ordinal);

        string src = Path.Combine(root, "src");
        return Directory
            .EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .SelectMany(file => AdrNumber().Matches(File.ReadAllText(file))
                .Select(m => (File: Path.GetFileName(file), Id: m.Value)))
            .Where(r => !known.Contains(r.Id))
            .Select(r => $"{r.File}: {r.Id} does not exist")
            .ToList();
    }

    public static List<string> DueForReview(string root, TimeProvider clock)
    {
        DateOnly today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        List<string> due = [];
        foreach (string path in AdrFiles(root))
        {
            const string field = "- Review by:";
            string? line = File.ReadAllLines(path)
                .FirstOrDefault(l => Starts(l, field));
            if (line is not null
                && DateOnly.TryParseExact(line[field.Length..].Trim(), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateOnly reviewBy)
                && reviewBy <= today)
            {
                due.Add(Path.GetFileName(path));
            }
        }

        return due;
    }

    private static string[] AdrFiles(string root) =>
        [.. Directory.GetFiles(Path.Combine(root, "docs", "adr"), "????-*.md")
            .Order(StringComparer.Ordinal)];

    private static string LineAfter(string[] lines, string heading) =>
        lines.SkipWhile(l => l != heading).Skip(1)
            .FirstOrDefault(l => l.Length > 0) ?? "";

    private static bool Starts(string line, string prefix) =>
        line.StartsWith(prefix, StringComparison.Ordinal);

    [GeneratedRegex(@"ADR-\d{4}")]
    private static partial Regex AdrNumber();
}
