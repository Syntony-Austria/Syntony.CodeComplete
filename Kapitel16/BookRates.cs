using System.Collections.Frozen;

namespace Library.Lending;

public enum BookKind
{
    Regular,
    Reference,
    Childrens,
    Journal,
}

public static class BookRates
{
    private static readonly FrozenDictionary<BookKind, decimal> s_factors =
        new Dictionary<BookKind, decimal>
        {
            { BookKind.Regular, 1m },
            { BookKind.Reference, 2m },
            { BookKind.Childrens, 0.5m },
            { BookKind.Journal, 1.5m },
        }.ToFrozenDictionary();

    public static IReadOnlyCollection<BookKind> Covered => s_factors.Keys;

    public static decimal FactorFor(BookKind kind) => s_factors[kind];
}
