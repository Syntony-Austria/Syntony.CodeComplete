namespace Library.Lending;

// New this week, written by a newcomer who joined last month.
public sealed class ReturnDesk(TimeProvider clock)
{
    public int FeeOnReturn(DateOnly dueDate)
    {
        DateOnly today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        int daysLate = today.DayNumber - dueDate.DayNumber;
        return Math.Max(0, daysLate - 3) * 25;
    }
}
