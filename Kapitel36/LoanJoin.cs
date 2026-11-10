namespace Library.Lending;

public sealed record Member(int Id, string Name);

public sealed record Loan(int MemberId, string Title);

public static class LoanJoin
{
    // Clean, short, and quadratic: for each loan, search all members.
    public static List<string> Slow(List<Member> members, List<Loan> loans) =>
        loans.Select(loan =>
            members.First(member => member.Id == loan.MemberId).Name).ToList();

    public static List<string> Fast(List<Member> members, List<Loan> loans)
    {
        Dictionary<int, string> names =
            members.ToDictionary(member => member.Id, member => member.Name);
        return loans.Select(loan => names[loan.MemberId]).ToList();
    }
}
