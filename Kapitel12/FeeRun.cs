namespace Library.Lending;

public static class FeeRun
{
    private const int GraceDays = 3;
    private const int CentsPerDay = 25;

    public static IReadOnlyList<Loan> FindDue(IReadOnlyList<Loan> loans) =>
        [.. loans.Where(loan => !loan.IsStaff && loan.DaysLate > GraceDays)];

    public static int TotalCents(IReadOnlyList<Loan> dueLoans) =>
        dueLoans.Sum(loan => loan.DaysLate * CentsPerDay);
}
