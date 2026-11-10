namespace Library.Lending;

// Deliberately wrong: split by time, so both know the format.
public static class LoanFileWriterTangled
{
    public static string Write(IEnumerable<(string Member, string Isbn)> loans) =>
        string.Join("\n", loans.Select(loan => loan.Member + ";" + loan.Isbn));
}

public static class LoanFileReaderTangled
{
    public static IReadOnlyList<(string Member, string Isbn)> Read(string text) =>
    [
        .. text.Split('\n').Select(line => line.Split(';'))
            .Select(parts => (parts[0], parts[1])),
    ];
}
