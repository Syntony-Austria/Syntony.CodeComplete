namespace Library.Lending;

public sealed class Catalog
{
    private readonly Dictionary<string, Book> _byIsbn = [];

    public int Count => _byIsbn.Count;

    public void Add(Book book) => _byIsbn[book.Isbn] = book;

    public string? TitleOf(string isbn) =>
        _byIsbn.TryGetValue(isbn, out Book? book) ? book.Title : null;

    public IReadOnlyList<string> SortedTitles() =>
        [.. _byIsbn.Values.Select(book => book.Title).Order()];
}

public static class ShelfDesk
{
    public static void Add(Catalog catalog, Book book) => catalog.Add(book);

    public static string? TitleOf(Catalog catalog, string isbn) =>
        catalog.TitleOf(isbn);

    public static IReadOnlyList<string> Sorted(Catalog catalog) =>
        catalog.SortedTitles();

    public static int Count(Catalog catalog) => catalog.Count;
}
