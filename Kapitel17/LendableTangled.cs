namespace Library.Lending;

public class LendableBook(string title)
{
    public string Title { get; } = title;

    public virtual int LoanDays() => 28;
}

// Deliberately wrong: it looks like a book, but it breaks the promise of one.
public sealed class ReferenceBookTangled(string title) : LendableBook(title)
{
    public override int LoanDays() =>
        throw new NotSupportedException("Reference books stay in the library.");
}

public sealed record Book(string Title, int? LoanDays)
{
    public bool IsLendable => LoanDays is not null;
}
