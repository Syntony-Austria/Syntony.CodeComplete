namespace Library.Lending;

public static class OverdueFeeRevised
{
    private const int GraceDays = 3;
    private const decimal FeePerDay = 0.25m;

    public static decimal For(Loan loan, MemberKind member, DateOnly today)
    {
        int daysLate = today.DayNumber - loan.DueDate.DayNumber;
        if (member == MemberKind.Staff || daysLate <= GraceDays)
        {
            return 0m;
        }

        decimal fullFee = Math.Min(
            daysLate * FeePerDay * BookFactor(loan), loan.BookPrice);
        return fullFee * MemberFactor(member);
    }

    private static decimal BookFactor(Loan loan) => loan.IsReference ? 2m : 1m;

    private static decimal MemberFactor(MemberKind member) =>
        member == MemberKind.Child ? 0.5m : 1m;
}
