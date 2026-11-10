using System.Globalization;

namespace Library.Lending;

// One finished piece of work: when it was started and when it was done.
public sealed record WorkItem(string Id, DateOnly Started, DateOnly Finished)
{
    public int Days => Finished.DayNumber - Started.DayNumber;

    public string ToCsv() => $"{Id},{Started:yyyy-MM-dd},{Finished:yyyy-MM-dd}";

    public static WorkItem FromCsv(string line)
    {
        string[] f = line.Split(',');
        return new WorkItem(f[0], Date(f[1]), Date(f[2]));
    }

    private static DateOnly Date(string s) =>
        DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
