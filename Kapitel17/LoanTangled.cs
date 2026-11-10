namespace Library.Lending;

public sealed class LoanTangled
{
    public string MemberId { get; set; } = "";

    public DateOnly DueDate { get; set; }

    public int RenewalsSoFar { get; set; }

    public bool IsReturned { get; set; }
}

public static class RenewalServiceTangled
{
    public static bool Renew(LoanTangled loan, DateOnly today, bool someoneWaiting)
    {
        if (loan.IsReturned || loan.RenewalsSoFar >= 2 || someoneWaiting)
        {
            return false;
        }

        if (today > loan.DueDate.AddDays(14))
        {
            return false;
        }

        loan.RenewalsSoFar++;
        loan.DueDate = loan.DueDate.AddDays(28);
        return true;
    }
}
