namespace Library.Lending;

public sealed class OverdueBoard
{
    public event Action<string>? LoanOverdue;

    public int ListenerCount => LoanOverdue?.GetInvocationList().Length ?? 0;

    public void Raise(string loanId) => LoanOverdue?.Invoke(loanId);
}

public sealed class ReminderScreen : IDisposable
{
    private readonly OverdueBoard _board;

    public ReminderScreen(OverdueBoard board)
    {
        _board = board;
        _board.LoanOverdue += Show;
    }

    public int Shown { get; private set; }

    public void Dispose() => _board.LoanOverdue -= Show;

    private void Show(string loanId) => Shown++;
}
