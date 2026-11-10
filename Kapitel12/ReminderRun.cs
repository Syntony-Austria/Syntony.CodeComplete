namespace Library.Lending;

public static class ReminderRun
{
    private const int GraceDays = 3;
    private const int CentsPerDay = 25;
    private const int CentsPerEuro = 100;

    public static string Build(IReadOnlyList<Loan> loans)
    {
        List<Loan> dueLoans =
            [.. loans.Where(loan => !loan.IsStaff && loan.DaysLate > GraceDays)];

        if (dueLoans.Count == 0)
        {
            return "";
        }

        int totalCents = dueLoans.Sum(loan => loan.DaysLate * CentsPerDay);
        int wholeEuros = totalCents / CentsPerEuro;

        return dueLoans.Count + " reminders, " + wholeEuros + " euros";
    }
}
