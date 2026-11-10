namespace Library.Lending;

public sealed record OpenLoan(string Member, DateOnly Due);

public sealed record Reminder(string Member, int DaysLate);

// The rule without a clock, a store or a mailer. It needs no double at all.
public static class ReminderPlan
{
    private const int GraceDays = 3;

    public static IReadOnlyList<Reminder> For(
        IEnumerable<OpenLoan> loans, DateOnly today) =>
        loans
            .Select(loan =>
                new Reminder(loan.Member, today.DayNumber - loan.Due.DayNumber))
            .Where(reminder => reminder.DaysLate > GraceDays)
            .OrderByDescending(reminder => reminder.DaysLate)
            .ToArray();
}
