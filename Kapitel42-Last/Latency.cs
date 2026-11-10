namespace Library.Lending;

public sealed record LatencySummary(
    int Count, double Mean, double P50, double P95, double P99, double Max);

public static class Latency
{
    // Nearest rank: the smallest value with at least `percent` percent
    // of all values at or below it. No interpolation, so the result is
    // always a time that some request really had.
    public static double Percentile(IReadOnlyList<double> sorted, int percent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(percent, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, 100);
        int rank = (percent * sorted.Count + 99) / 100;
        return sorted[rank - 1];
    }

    public static LatencySummary Summarize(IEnumerable<double> values)
    {
        double[] sorted = [.. values.Order()];
        return new LatencySummary(sorted.Length, sorted.Average(),
            Percentile(sorted, 50), Percentile(sorted, 95),
            Percentile(sorted, 99), sorted[^1]);
    }
}
