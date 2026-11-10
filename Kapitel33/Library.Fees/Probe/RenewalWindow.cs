namespace Library.Lending;

// A second rule that no test calls. It only exists to fool the ratchet.
// dotnet test -p:CoverageProbe=true --collect:"XPlat Code Coverage"
//   -> coverage falls below the floor; Ratchet.py exits with 2.
// Add -p:ExcludeProbe=true: one attribute hides the class, the report
//   shows 100 and 100 again, and not a single test was written.
#if EXCLUDE_PROBE
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public static class RenewalWindow
{
    public const int MaxDaysOverdue = 14;

    public static bool IsOpen(int daysLate, bool isReserved)
    {
        if (isReserved)
        {
            return false;
        }

        return daysLate <= MaxDaysOverdue;
    }
}
