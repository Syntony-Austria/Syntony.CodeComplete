namespace Library.Lending;

// Nothing in the signature promises an order.
public static class TitlesV1
{
    public static IReadOnlyCollection<string> All() => ["Faust", "Woyzeck"];
}

public static class TitlesV2
{
    public static IReadOnlyCollection<string> All() => ["Woyzeck", "Faust"];
}

public static class TitleClient
{
    // Deliberately wrong: it relies on an order nobody promised.
    public static string FirstOf(IReadOnlyCollection<string> titles) =>
        titles.First();
}
