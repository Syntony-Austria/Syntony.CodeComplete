using System.Globalization;

namespace Library.Lending;

public abstract record LoanState;

public sealed record OnShelf : LoanState;

public sealed record Lent(MemberId Member, DateOnly DueDate) : LoanState;

public sealed record Lost(DateOnly ReportedOn) : LoanState;

public static class LoanStateText
{
    public static string Describe(LoanState state) => state switch
    {
        OnShelf => "on the shelf",
        Lent lent => lent.Member.Value + " until " + Day(lent.DueDate),
        Lost lost => "lost on " + Day(lost.ReportedOn),
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };

    private static string Day(DateOnly day) =>
        day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
