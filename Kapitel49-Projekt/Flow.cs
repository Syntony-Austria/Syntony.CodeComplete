namespace Library.Lending;

public sealed record LittleCheck(double MeanWip, double PerDay, double MeanDays)
{
    public double Product => PerDay * MeanDays;
}

// Flow numbers from a list of finished pieces.
public static class Flow
{
    // How many pieces were finished in each week, starting on a Monday.
    public static int[] WeeklyThroughput(
        IEnumerable<WorkItem> items, DateOnly monday, int weeks)
    {
        int[] count = new int[weeks];
        foreach (WorkItem item in items.Where(i => i.Finished >= monday))
        {
            int week = (item.Finished.DayNumber - monday.DayNumber) / 7;
            if (week < weeks)
            {
                count[week]++;
            }
        }

        return count;
    }

    // L counted day by day; lambda and W from the pieces finished inside.
    public static LittleCheck Little(
        IReadOnlyList<WorkItem> items, DateOnly from, DateOnly to)
    {
        int days = to.DayNumber - from.DayNumber;
        double wip = Enumerable.Range(0, days)
            .Select(from.AddDays)
            .Average(d => items.Count(i => i.Started <= d && d < i.Finished));
        List<WorkItem> done =
            [.. items.Where(i => i.Finished >= from && i.Finished < to)];
        return new LittleCheck(
            wip, done.Count / (double)days, done.Average(i => i.Days));
    }

    // Every `size` pieces in finishing order become one big piece.
    public static List<WorkItem> Merge(IEnumerable<WorkItem> items, int size) =>
        [.. items.OrderBy(i => i.Finished).Chunk(size)
            .Where(c => c.Length == size)
            .Select(c => new WorkItem(c[0].Id, c.Min(i => i.Started),
                c[^1].Finished))];
}
