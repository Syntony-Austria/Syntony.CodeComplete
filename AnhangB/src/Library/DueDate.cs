using System.Globalization;

namespace Library;

public sealed class DueDate(TimeProvider clock)
{
    private readonly TimeProvider _clock = clock;

    public bool IsOverdue(DateOnly due)
    {
        DateOnly today = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
        return today > due;
    }

    public string Show(DateOnly due)
    {
        return due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
