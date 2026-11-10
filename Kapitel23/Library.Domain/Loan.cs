namespace Library.Domain;

public sealed record Loan(string MemberId, DateOnly DueDate);

// The domain says what it needs, in its own words.
public interface ILoanStore
{
    IReadOnlyList<Loan> OpenLoans();
}

public static class Reminders
{
    private const int GraceDays = 3;

    public static IReadOnlyList<string> Due(ILoanStore store, DateOnly today) =>
    [
        .. store.OpenLoans()
            .Where(loan => loan.DueDate.AddDays(GraceDays) < today)
            .Select(loan => loan.MemberId),
    ];
}
