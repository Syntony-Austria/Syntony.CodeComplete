namespace Library.Lending;

public sealed class LibraryDeskTangled
{
    private readonly List<string> _waiting = [];
    private readonly List<string> _reminders = [];
    private decimal _cash;

    public void Join(string memberId) => _waiting.Add(memberId);

    public int WaitingCount() => _waiting.Count;

    public void Remind(string memberId) => _reminders.Add(memberId);

    public int ReminderCount() => _reminders.Count;

    public void TakeFee(decimal amount) => _cash += amount;

    public decimal Cash() => _cash;
}
