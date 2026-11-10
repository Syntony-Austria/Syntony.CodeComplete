namespace Library.Lending;

public sealed class FeeRunTangled
{
    private const int GraceDays = 3;
    private const int CentsPerDay = 25;

    private List<Loan> _dueLoans = [];

    public void Load(IReadOnlyList<Loan> loans) =>
        _dueLoans =
            [.. loans.Where(loan => !loan.IsStaff && loan.DaysLate > GraceDays)];

    public int Compute() => _dueLoans.Sum(loan => loan.DaysLate * CentsPerDay);
}
