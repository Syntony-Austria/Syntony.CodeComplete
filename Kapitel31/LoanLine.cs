namespace Library.Lending;

public static class LoanLine
{
    public static string Format(string member, string title) =>
        member + ";" + title;

    // First attempt: repairs the report and nothing else.
    public static (string Member, string Title) ParseFirstFix(string line)
    {
        string[] parts = line.Split(';', 2);
        return (parts[0], parts[1]);
    }

    // The format itself is the bug: escape the separator when writing.
    public static string FormatSafe(string member, string title) =>
        Escape(member) + ";" + Escape(title);

    public static (string Member, string Title) ParseSafe(string line)
    {
        int separator = line.IndexOf(';');
        return (Unescape(line[..separator]), Unescape(line[(separator + 1)..]));
    }

    private static string Escape(string text) =>
        text.Replace("%", "%25").Replace(";", "%3B");

    private static string Unescape(string text) =>
        text.Replace("%3B", ";").Replace("%25", "%");
}
