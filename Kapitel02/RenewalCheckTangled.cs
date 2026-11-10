namespace Library.Lending;

public static class RenewalCheckTangled
{
    public static bool Chk(int n, DateOnly d, DateOnly t, bool h, bool p)
    {
        // check n
        if (n >= 2)
        {
            return false;
        }

        // check date
        if (t > d.AddDays(14))
        {
            return false;
        }

        return !h || p;
    }
}
