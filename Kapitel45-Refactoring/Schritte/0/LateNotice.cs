namespace Library.Lending;

public static class LateNotice
{
    public static string Text(string n, int d, bool c)
    {
        string s;
        if (d <= 3)
        {
            s = "";
        }
        else
        {
            if (d > 80)
            {
                s = "Letzte Mahnung an " + n;
            }
            else
            {
                if (c)
                {
                    s = "Liebe Eltern von " + n + ", bitte Buch zurueck.";
                }
                else
                {
                    s = "Mahnung an " + n;
                }
            }
        }

        return s;
    }
}
