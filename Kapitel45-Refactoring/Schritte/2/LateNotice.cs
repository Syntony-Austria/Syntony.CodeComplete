namespace Library.Lending;

public static class LateNotice
{
    // Step 2: guard clauses instead of nesting.
    public static string Text(string member, int daysLate, bool isChild)
    {
        if (daysLate <= 3)
        {
            return "";
        }

        if (daysLate > 80)
        {
            return "Letzte Mahnung an " + member;
        }

        if (isChild)
        {
            return "Liebe Eltern von " + member + ", bitte Buch zurueck.";
        }

        return "Mahnung an " + member;
    }
}
