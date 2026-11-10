namespace Library.Lending;

public static class FeeStepsTangled
{
    public static int CentsPerDay(int daysLate)
    {
        if (daysLate <= 3)
        {
            return 0;
        }
        else if (daysLate <= 14)
        {
            return 25;
        }
        else if (daysLate <= 30)
        {
            return 50;
        }
        else if (daysLate <= 90)
        {
            return 75;
        }
        else
        {
            return 100;
        }
    }
}
