namespace Library.Lending;

public enum MemberKind
{
    Adult,
    Child,
    Staff,
}

public sealed record Loan(DateOnly DueDate, bool IsReference, decimal BookPrice);

public static class OverdueFee
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

        decimal fee = daysLate * FeePerDay
            * BookFactor(loan) * MemberFactor(member);
        return Math.Min(fee, loan.BookPrice);
    }

    private static decimal BookFactor(Loan loan) => loan.IsReference ? 2m : 1m;

    private static decimal MemberFactor(MemberKind member) =>
        member == MemberKind.Child ? 0.5m : 1m;
}
