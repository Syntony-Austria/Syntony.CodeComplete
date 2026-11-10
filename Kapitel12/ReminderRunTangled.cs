namespace Library.Lending;

public static class ReminderRunTangled
{
    public static string Build(IReadOnlyList<Loan> loans)
    {
        int n = 0;
        int total = 0;
        string text = "";
        bool any = false;

        foreach (Loan loan in loans)
        {
            if (!loan.IsStaff && loan.DaysLate > 3)
            {
                total += loan.DaysLate * 25;
                n++;
                any = true;
            }
        }

        if (any)
        {
            text = n + " reminders, ";
        }

        total = total / 100;

        if (any)
        {
            text += total + " euros";
        }

        return text;
    }
}
