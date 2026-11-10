namespace Library.Lending;

public sealed class FakeDbException(int number) : Exception("db error")
{
    public int Number { get; } = number;
}

// Deliberately wrong: the caller must know the database to read the answer.
public sealed class LeakyStore
{
    public void Save(string text) => throw new FakeDbException(2627);
}

public sealed class DuplicateLoanException(string message, Exception inner)
    : Exception(message, inner);

public sealed class TightStore
{
    private const int UniqueViolation = 2627;

    public void Save(string text)
    {
        try
        {
            throw new FakeDbException(UniqueViolation);
        }
        catch (FakeDbException ex) when (ex.Number == UniqueViolation)
        {
            throw new DuplicateLoanException("Loan exists: " + text, ex);
        }
    }
}
