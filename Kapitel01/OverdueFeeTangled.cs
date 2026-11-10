namespace Library.Lending;

public static class OverdueFeeTangled
{
    public static decimal Calc(
        DateOnly due, DateOnly now, int kind,
        bool reference, decimal price, bool waived)
    {
        decimal r = 0m;
        if (!waived)
        {
            if (kind != 2)
            {
                if (now > due)
                {
                    int d = now.DayNumber - due.DayNumber;
                    if (d > 3)
                    {
                        r = d * 0.25m;
                        if (reference)
                        {
                            r = r * 2m;
                        }

                        if (kind == 1)
                        {
                            r = r / 2m;
                        }

                        if (r > price)
                        {
                            r = price;
                        }
                    }
                }
            }
        }

        return r;
    }
}
