namespace Library.Lending;

// A temporary solution with an end date: the test turns red when it is over.
public sealed record Sunset(string What, DateOnly Until)
{
    public bool IsOver(DateOnly today) => today > Until;
}

public static class Sunsets
{
    public static readonly IReadOnlyList<Sunset> All =
    [
        new("Read format version 1", new DateOnly(2027, 12, 31)),
    ];
}
