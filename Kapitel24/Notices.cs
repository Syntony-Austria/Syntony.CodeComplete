namespace Library.Lending;

public static class Notices
{
    // Old name, still there, with the way out in its message.
    [Obsolete("Use Compose. Will be removed in version 3.0.")]
    public static string Text(string memberId) => Compose(memberId, "de");

    public static string Compose(string memberId, string language) =>
        language + ":" + memberId;

    // The second overload keeps old compiled callers alive.
    public static int FeeFor(int days) => FeeFor(days, 25);

    public static int FeeFor(int days, int centsPerDay) => days * centsPerDay;
}
