namespace Library.Lending;

// Deliberately incomplete. dotnet build -p:SwitchProbe=true must fail with
// CS8509 (enum, a named case missing), CS8524 (enum, only unnamed values
// missing) and CS8509 (records, all three cases there, no _ arm).
public static class SwitchProbe
{
    public static string MissingNamedCase(MemberRank rank) => rank switch
    {
        MemberRank.Regular => "regular",
    };

    public static string AllNamedCases(MemberRank rank) => rank switch
    {
        MemberRank.Regular => "regular",
        MemberRank.Prioritised => "prioritised",
    };

    public static string AllStates(LoanState state) => state switch
    {
        OnShelf => "on the shelf",
        Lent => "lent",
        Lost => "lost",
    };
}
