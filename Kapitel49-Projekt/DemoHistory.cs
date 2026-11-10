namespace Library.Lending;

// An invented year of the online reservation project. On each working
// day a new piece starts with chance 0.6, in the summer weeks 29 to 31
// with chance 0.15. Cycle times are lognormal around six days.
public static class DemoHistory
{
    public static readonly DateOnly FirstMonday = new(2025, 1, 6);
    public const int Weeks = 52;

    public static List<WorkItem> Create(int seed = 49)
    {
        Random random = new(seed);
        List<WorkItem> items = [];
        for (int day = 0; day < Weeks * 7; day++)
        {
            DateOnly date = FirstMonday.AddDays(day);
            double chance = day / 7 is >= 29 and <= 31 ? 0.15 : 0.6;
            if (IsWeekend(date) || random.NextDouble() >= chance)
            {
                continue;
            }

            double z = Math.Sqrt(-2 * Math.Log(1 - random.NextDouble()))
                * Math.Cos(2 * Math.PI * random.NextDouble());
            int days = Math.Max(1, (int)Math.Round(6 * Math.Exp(0.8 * z)));
            DateOnly done = date.AddDays(days);
            while (IsWeekend(done))
            {
                done = done.AddDays(1);
            }

            items.Add(new WorkItem($"LND-{100 + items.Count}", date, done));
        }

        return items;
    }

    private static bool IsWeekend(DateOnly d) =>
        d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}
