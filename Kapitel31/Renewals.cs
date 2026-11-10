namespace Library.Lending;

public static class Renewals
{
    private const int MaxRenewals = 2;

    public static bool MayRenew(int renewedSoFar, bool isReserved) =>
        renewedSoFar < MaxRenewals && !isReserved;
}
