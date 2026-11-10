namespace Library.Lending;

public static class FeeSteps
{
    private sealed record Step(int FromDay, int CentsPerDay);

    // Each step holds from its first day up to the next step. Keep it sorted.
    private static readonly Step[] s_steps =
    [
        new(int.MinValue, 0),
        new(4, 25),
        new(15, 50),
        new(31, 75),
        new(91, 100),
    ];

    public static int CentsPerDay(int daysLate) =>
        s_steps.Last(step => step.FromDay <= daysLate).CentsPerDay;

    public static bool IsSorted() =>
        s_steps.Zip(s_steps.Skip(1))
            .All(pair => pair.First.FromDay < pair.Second.FromDay);
}
