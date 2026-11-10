namespace Library.Lending;

// Deliberately wrong. dotnet build -p:GenericIdProbe=true must fail with
// CS1503: Id<Member> and Id<Book> are two different types.
public readonly record struct Id<TEntity>(string Value);

public sealed class Member;

public sealed class Book;

public static class GenericIdProbe
{
    public static string Describe(Id<Member> member, Id<Book> book) =>
        member.Value + " has " + book.Value;

    public static string Swapped(Id<Member> member, Id<Book> book) =>
        Describe(book, member);
}
