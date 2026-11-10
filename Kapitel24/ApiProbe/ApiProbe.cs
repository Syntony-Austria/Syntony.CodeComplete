// The public surface lives in PublicAPI.Shipped.txt; a change shows in a diff.
// dotnet build ApiProbe -> silent, the surface matches the file.
// dotnet build ApiProbe -p:NewMemberProbe=true -> RS0016 for More.
// dotnet build ApiProbe -p:RenameProbe=true -> RS0016 for Named in this
// file and RS0017 in PublicAPI.Shipped.txt, line 3 (old signature).
namespace Library.Lending;

public static class Fees
{
#if RENAME_PROBE
    public static int Named(int late) => late;
#else
    public static int Named(int daysLate) => daysLate;
#endif

#if NEW_MEMBER_PROBE
    public static int More() => 2;
#endif
}
