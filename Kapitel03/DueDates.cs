namespace Library.Lending;

public sealed class DueDates(TimeProvider clock)
{
    private const int LoanPeriodDays = 28;

    public DateOnly ForNewLoan() =>
        DateOnly.FromDateTime(clock.GetLocalNow().Date).AddDays(LoanPeriodDays);
}
