namespace Library.Lending;

// Blocks only what is new; the old findings are listed in a file.
public static class Baseline
{
    public static List<string> Read(string path) =>
        [.. File.ReadAllLines(path)
            .Where(l => l.Length > 0 && !l.StartsWith('#'))];

    public static List<Finding> NewFindings(
        IEnumerable<Finding> current, IEnumerable<string> baseline)
    {
        Dictionary<string, int> allowed = baseline
            .GroupBy(k => k, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        List<Finding> fresh = [];
        foreach (Finding f in current)
        {
            if (allowed.TryGetValue(f.Key, out int left) && left > 0)
            {
                allowed[f.Key] = left - 1;
            }
            else
            {
                fresh.Add(f);
            }
        }

        return fresh;
    }

    // Entries nobody needs any more: the baseline may only shrink.
    public static List<string> Stale(
        IEnumerable<Finding> current, IEnumerable<string> baseline)
    {
        HashSet<string> seen = current.Select(f => f.Key)
            .ToHashSet(StringComparer.Ordinal);
        return [.. baseline.Where(k => !seen.Contains(k))];
    }
}
