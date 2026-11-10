namespace Library.Lending;

// One row while the change is under way: the old column and the new ones.
public sealed class MemberRow
{
    public string? FullName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}

public static class MemberNameMigration
{
    // Expand: write both, so old readers keep working.
    public static void Write(MemberRow row, string first, string last)
    {
        row.FirstName = first;
        row.LastName = last;
        row.FullName = first + " " + last;
    }

    // Migrate: read the new form, fall back to the old one.
    public static string Display(MemberRow row) =>
        row.LastName is null
            ? row.FullName ?? ""
            : row.FirstName + " " + row.LastName;

    // Backfill: rows no one has touched yet. Running it twice changes nothing.
    public static void Backfill(MemberRow row)
    {
        if (row.LastName is not null || row.FullName is null)
        {
            return;
        }

        int space = row.FullName.LastIndexOf(' ');
        row.FirstName = space < 0 ? "" : row.FullName[..space];
        row.LastName = row.FullName[(space + 1)..];
    }
}
