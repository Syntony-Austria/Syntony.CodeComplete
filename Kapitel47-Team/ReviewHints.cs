namespace Library.Lending;

public sealed record Candidate(string Hint, int Reviews, string First, string Last);

// Counts how many reviews raised the same hint; three make a candidate.
public static class ReviewHints
{
    public const int Threshold = 3;

    // One line per hint: date, review, hint, separated by tabs.
    public static List<Candidate> Candidates(IEnumerable<string> lines)
    {
        return [.. lines
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .Select(l => l.Split('\t'))
            .GroupBy(p => p[2], StringComparer.Ordinal)
            .Select(g => new Candidate(
                g.Key,
                g.Select(p => p[1]).Distinct(StringComparer.Ordinal).Count(),
                g.Select(p => p[0]).Min(StringComparer.Ordinal)!,
                g.Select(p => p[0]).Max(StringComparer.Ordinal)!))
            .Where(c => c.Reviews >= Threshold)
            .OrderByDescending(c => c.Reviews)];
    }
}
