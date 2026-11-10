namespace Library.Lending;

public sealed class DueDates(TimeProvider clock)
{
    private const int LoanDays = 28;

    public DateOnly ForNewLoan() =>
        DateOnly.FromDateTime(clock.GetLocalNow().DateTime).AddDays(LoanDays);

    public bool IsOverdue(DateOnly due) =>
        DateOnly.FromDateTime(clock.GetLocalNow().DateTime) > due;
}

public sealed class ReminderWait(TimeProvider clock)
{
    public async Task<string> AfterOneDayAsync(string member)
    {
        await Task.Delay(TimeSpan.FromDays(1), clock);
        return "Reminder for " + member;
    }
}
