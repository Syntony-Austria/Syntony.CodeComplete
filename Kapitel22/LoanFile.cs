namespace Library.Lending;

// One secret, one place: only this class knows how a loan looks on disk.
public static class LoanFile
{
    private const char Separator = ';';

    public static string Write(IEnumerable<(string Member, string Isbn)> loans) =>
        string.Join(
            "\n", loans.Select(loan => loan.Member + Separator + loan.Isbn));

    public static IReadOnlyList<(string Member, string Isbn)> Read(string text) =>
    [
        .. text.Split('\n').Select(line => line.Split(Separator))
            .Select(parts => (parts[0], parts[1])),
    ];
}
