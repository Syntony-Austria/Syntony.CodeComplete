namespace Library.Lending;

public static class ErrorBudget
{
    // SLI: the share of good events. Here: lookups at or under the goal.
    public static double Sli(IReadOnlyCollection<double> ms, double goalMs) =>
        (double)ms.Count(m => m <= goalMs) / ms.Count;

    // Time a service may be bad in `window` and still meet `objective`.
    public static TimeSpan Allowed(double objective, TimeSpan window) =>
        window * (1 - objective);

    // Share of the budget still left: 1 = untouched, 0 = used up.
    public static double Left(double objective, long good, long total)
    {
        double allowedBad = (1 - objective) * total;
        return 1 - (total - good) / allowedBad;
    }
}
