namespace Library.Lending;

// The one place for overdue fees (team rule since 2025).
public static class FeeSchedule
{
    private const int GraceDays = 3;
    private const int CentsPerDay = 25;
    private const int CapInCents = 2000;

    public static int FeeInCents(int daysLate)
    {
        int chargedDays = Math.Max(0, daysLate - GraceDays);
        return Math.Min(chargedDays * CentsPerDay, CapInCents);
    }
}
