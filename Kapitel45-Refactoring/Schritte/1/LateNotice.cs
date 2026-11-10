namespace Library.Lending;

public static class LateNotice
{
    // Step 1: rename only. The compiler and the tests watch.
    public static string Text(string member, int daysLate, bool isChild)
    {
        string text;
        if (daysLate <= 3)
        {
            text = "";
        }
        else
        {
            if (daysLate > 80)
            {
                text = "Letzte Mahnung an " + member;
            }
            else
            {
                if (isChild)
                {
                    text = "Liebe Eltern von " + member + ", bitte Buch zurueck.";
                }
                else
                {
                    text = "Mahnung an " + member;
                }
            }
        }

        return text;
    }
}
