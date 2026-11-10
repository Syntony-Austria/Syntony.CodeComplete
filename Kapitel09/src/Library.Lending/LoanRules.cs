namespace Library.Lending;

public static class LoanRules
{
    private const int GraceDays = 3;
    private const int CentsPerDay = 25;

    public static int FeeInCents(int daysLate) =>
        daysLate <= GraceDays ? 0 : daysLate * CentsPerDay;
}
