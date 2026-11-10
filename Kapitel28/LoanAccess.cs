namespace Library.Lending;

public enum Role
{
    Member,
    Librarian,
}

public sealed record Requester(string MemberId, Role Role);

public sealed record LoanView(int Id, string MemberId, string Title);

public sealed class LoanBook(IReadOnlyList<LoanView> loans)
{
    // Deliberately wrong: the id comes from outside and nobody asks whose it is.
    public LoanView? GetTangled(int id) =>
        loans.FirstOrDefault(loan => loan.Id == id);

    // Not allowed and not there look the same from the outside.
    public LoanView? Get(Requester requester, int id)
    {
        LoanView? loan = loans.FirstOrDefault(candidate => candidate.Id == id);
        return loan is not null && Policy.MayRead(requester, loan) ? loan : null;
    }
}

public static class Policy
{
    private static readonly Dictionary<Role, Func<Requester, LoanView, bool>>
        s_read = new()
        {
            { Role.Librarian, (_, _) => true },
            { Role.Member, (who, loan) => who.MemberId == loan.MemberId },
        };

    // Whatever is not listed is refused.
    public static bool MayRead(Requester requester, LoanView loan) =>
        s_read.TryGetValue(
            requester.Role, out Func<Requester, LoanView, bool>? rule)
        && rule(requester, loan);
}
