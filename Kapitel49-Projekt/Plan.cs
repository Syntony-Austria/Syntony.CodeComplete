namespace Library.Lending;

// The usual plan: the work that is left, divided by the mean of past weeks.
public static class Plan
{
    public static int ByAverage(int[] pastWeeks, int remaining) =>
        (int)Math.Ceiling(remaining / pastWeeks.Average());
}
