namespace Library.Lending;

// Deliberately wrong. Built only with -p:TypeProbe=true: swapped arguments.
public static class TypeProbe
{
    public static string Swapped(MemberId member, BookId book) =>
        LoansById.Describe(book, member);
}
