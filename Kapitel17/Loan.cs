namespace Library.Lending;

public sealed class Loan(string memberId, DateOnly dueDate)
{
    private const int MaxRenewals = 2;
    private const int MaxDaysOverdue = 14;
    private const int LoanPeriodDays = 28;

    private int _renewalsSoFar;

    public string MemberId { get; } = memberId;

    public DateOnly DueDate { get; private set; } = dueDate;

    public bool IsReturned { get; private set; }

    public bool TryRenew(DateOnly today, bool someoneWaiting)
    {
        bool mayRenew = !IsReturned
            && _renewalsSoFar < MaxRenewals
            && !someoneWaiting
            && today <= DueDate.AddDays(MaxDaysOverdue);
        if (!mayRenew)
        {
            return false;
        }

        _renewalsSoFar++;
        DueDate = DueDate.AddDays(LoanPeriodDays);
        return true;
    }

    public void Return() => IsReturned = true;
}
