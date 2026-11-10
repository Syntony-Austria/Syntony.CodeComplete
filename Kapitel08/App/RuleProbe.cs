namespace Library.Lending;

// Deliberately wrong. Built only with -p:RuleProbe=true, to prove the rule bites.
public static class RuleProbe
{
    public static bool MayRenew(bool hasReservation, bool isPrioritised) =>
        !hasReservation || isPrioritised;
}
