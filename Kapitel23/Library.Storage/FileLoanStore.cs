using System.Globalization;
using Library.Domain;

namespace Library.Storage;

// The detail points at the rule, not the other way round.
public sealed class FileLoanStore(string path) : ILoanStore
{
    public IReadOnlyList<Loan> OpenLoans() =>
    [
        .. File.ReadAllLines(path)
            .Select(line => line.Split(';'))
            .Select(parts => new Loan(parts[0], Day(parts[1]))),
    ];

    private static DateOnly Day(string text) =>
        DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
