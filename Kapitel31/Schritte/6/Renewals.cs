namespace Library.Lending;

public static class Renewals
{
    public static bool MayRenew(int renewedSoFar, bool isReserved) =>
        renewedSoFar < 2 && !isReserved;
}
