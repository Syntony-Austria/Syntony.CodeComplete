namespace Library.Lending;

// Deliberately wrong. dotnet build -p:ImplicitProbe=true must SUCCEED: with
// an implicit conversion from string, the swapped call compiles silently.
public readonly record struct ImplicitMemberId(string Value)
{
    public static implicit operator ImplicitMemberId(string value) => new(value);
}

public readonly record struct ImplicitBookId(string Value)
{
    public static implicit operator ImplicitBookId(string value) => new(value);
}

public static class ImplicitProbe
{
    public static string Describe(ImplicitMemberId member, ImplicitBookId book) =>
        member.Value + " has " + book.Value;

    public static string Swapped() => Describe("b-17", "m-1");
}
