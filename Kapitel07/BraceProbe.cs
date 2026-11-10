namespace Library.Lending;

// Deliberately wrong. The second line looks guarded and is not.
public static class BraceProbe
{
    public static int FeeInCents(int daysLate)
    {
        int fee = 0;
        if (daysLate > 3)
            fee = 100;
            fee = fee + 25;
        return fee;
    }
}
