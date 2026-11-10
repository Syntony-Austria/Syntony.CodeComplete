namespace Library.Lending;

public static class DaysLate
{
    // The bug from the report: a whole 24 hours counted, not a calendar day.
    public static int Tangled(DateTimeOffset due, DateTimeOffset returned) =>
        Math.Max(0, (returned - due).Days);

    public static int Between(DateOnly due, DateOnly returned) =>
        Math.Max(0, returned.DayNumber - due.DayNumber);
}
