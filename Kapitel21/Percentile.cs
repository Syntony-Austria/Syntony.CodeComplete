namespace Library.Lending;

public static class Percentile
{
    public static double Of(IReadOnlyList<double> values, double share)
    {
        List<double> sorted = [.. values.Order()];
        int index = (int)Math.Ceiling(share * sorted.Count) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Count - 1)];
    }
}
