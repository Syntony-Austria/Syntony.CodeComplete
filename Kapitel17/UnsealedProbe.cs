namespace Library.Lending;

// An internal class nobody derives from, without sealed. Built only with
// -p:UnsealedProbe=true. Alone the build stays silent; together with
// -p:AnalysisMode=Recommended it reports CA1852.
internal class ShelfLabel
{
    public string Text { get; } = "A-1";
}

public static class UnsealedProbe
{
    public static string Label() => new ShelfLabel().Text;
}
