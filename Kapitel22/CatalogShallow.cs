namespace Library.Lending;

public sealed record Book(string Isbn, string Title);

// Deliberately wrong: the storage is the interface.
public sealed class CatalogShallow
{
    public List<Book> Books { get; } = [];

    public Dictionary<string, Book> ByIsbn { get; } = [];
}

public static class ShelfDeskShallow
{
    public static void Add(CatalogShallow catalog, Book book)
    {
        catalog.Books.Add(book);
        catalog.ByIsbn[book.Isbn] = book;
    }

    public static string? TitleOf(CatalogShallow catalog, string isbn) =>
        catalog.ByIsbn.TryGetValue(isbn, out Book? book) ? book.Title : null;

    public static IReadOnlyList<string> Sorted(CatalogShallow catalog) =>
        [.. catalog.Books.Select(book => book.Title).Order()];

    public static int Count(CatalogShallow catalog) => catalog.Books.Count;
}
