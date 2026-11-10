namespace Library.Lending;

public static class TitleSorter
{
    public static IReadOnlyList<string> Sorted(IEnumerable<string> titles) =>
        [.. titles.Order(InvariantOrder.Instance)];
}

// Visible in this file only: nobody else can come to depend on it.
file sealed class InvariantOrder : IComparer<string>
{
    public static readonly InvariantOrder Instance = new();

    public int Compare(string? left, string? right) =>
        string.Compare(left, right, StringComparison.InvariantCulture);
}
