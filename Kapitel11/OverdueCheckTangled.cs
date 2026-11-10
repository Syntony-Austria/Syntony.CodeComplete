namespace Library.Lending;

public sealed class OverdueCheckTangled
{
    public List<string> Reminders { get; } = [];

    // Reads like a question. It also writes.
    public bool IsOverdue(string loanId, int daysLate)
    {
        if (daysLate > 0)
        {
            Reminders.Add(loanId);
            return true;
        }

        return false;
    }
}
