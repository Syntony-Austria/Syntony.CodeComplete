namespace Library.Lending;

public enum ReminderKind
{
    Friendly,
    Firm,
    Final,
}

public static class ReminderTexts
{
    private static readonly Dictionary<ReminderKind, Func<string, string>>
        s_writers = new()
        {
            { ReminderKind.Friendly, title => $"Please return {title}." },
            { ReminderKind.Firm, title => $"{title} is overdue. Fees apply." },
            { ReminderKind.Final, Final },
        };

    public static IReadOnlyCollection<ReminderKind> Covered => s_writers.Keys;

    public static string Write(ReminderKind kind, string title) =>
        s_writers[kind](title);

    private static string Final(string title) =>
        $"Last reminder for {title}. We will invoice the book.";
}
