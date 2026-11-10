namespace Library.Lending;

public static class RenewalTable
{
    private static readonly
        Dictionary<(bool Late, bool Waiting, bool Prioritised), string>
        s_rows = new()
        {
            { (false, false, false), "renewed" },
            { (false, false, true), "renewed" },
            { (false, true, false), "reserved" },
            { (false, true, true), "renewed" },
            { (true, false, false), "too late" },
            { (true, false, true), "too late" },
            { (true, true, false), "too late" },
            { (true, true, true), "too late" },
        };

    public static int RowCount => s_rows.Count;

    public static string Decide(bool late, bool waiting, bool prioritised) =>
        s_rows[(late, waiting, prioritised)];
}
