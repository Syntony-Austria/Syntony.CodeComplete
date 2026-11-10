namespace Library.Lending;

public readonly record struct MemberId(string Value);

public readonly record struct BookId(string Value);

public static class LoansById
{
    public static string Describe(MemberId member, BookId book) =>
        member.Value + " has " + book.Value;

    public static string DescribeTangled(string memberId, string bookId) =>
        memberId + " has " + bookId;
}
