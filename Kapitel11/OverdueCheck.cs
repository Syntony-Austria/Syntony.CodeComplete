namespace Library.Lending;

public sealed class OverdueCheck
{
    private readonly List<string> _reminders = [];

    public int ReminderCount => _reminders.Count;

    public static bool IsOverdue(int daysLate) => daysLate > 0;

    public void SendReminder(string memberId) => _reminders.Add(memberId);
}
