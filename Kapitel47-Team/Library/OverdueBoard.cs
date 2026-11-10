namespace Library.Lending;

// Older than the rule; shows the expected fee on the board in the hall.
public static class OverdueBoard
{
    public static string Line(string title, int daysLate) =>
        $"{title}: {Math.Min(Math.Max(0, daysLate - 3) * 25, 2000)} cents";

    public static int Total(IEnumerable<int> daysLate) =>
        daysLate.Sum(d => Math.Min(Math.Max(0, d - 3) * 25, 2000));
}
