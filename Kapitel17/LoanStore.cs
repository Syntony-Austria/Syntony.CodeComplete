namespace Library.Lending;

public interface ILoanStore
{
    Loan? FindOpenLoan(string memberId, string bookId);

    void Save(Loan loan);
}

public sealed class RenewalDesk(ILoanStore store)
{
    public bool Renew(
        string memberId, string bookId, DateOnly today, bool someoneWaiting)
    {
        Loan? loan = store.FindOpenLoan(memberId, bookId);
        if (loan is null || !loan.TryRenew(today, someoneWaiting))
        {
            return false;
        }

        store.Save(loan);
        return true;
    }
}

public sealed class InMemoryLoanStore : ILoanStore
{
    private readonly Dictionary<(string Member, string Book), Loan> _loans = [];

    public int Saves { get; private set; }

    public void Add(string bookId, Loan loan) =>
        _loans[(loan.MemberId, bookId)] = loan;

    public Loan? FindOpenLoan(string memberId, string bookId) =>
        _loans.TryGetValue((memberId, bookId), out Loan? loan) && !loan.IsReturned
            ? loan
            : null;

    public void Save(Loan loan) => Saves++;
}
