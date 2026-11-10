namespace Library.Lending;

public sealed record LoanPeriod
{
    public LoanPeriod(int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(days);
        Days = days;
    }

    public int Days { get; init; }
}

public sealed class Counter(int count)
{
    public int Next() => ++count;
}
