namespace Library.Lending;

public static class LateNotice
{
    private const int GraceDays = 3;
    private const int LastNoticeAfterDays = 80;

    // Step 4, deliberately wrong: "while we are here", >= instead of >.
    public static string Text(string member, int daysLate, bool isChild)
    {
        if (daysLate <= GraceDays)
        {
            return "";
        }

        if (daysLate >= LastNoticeAfterDays)
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
