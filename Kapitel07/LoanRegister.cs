namespace Library.Lending;

public sealed class LoanRegister
{
    private readonly List<string> _openLoans = [];

    public int OpenCount => _openLoans.Count;

    public void Open(string loanId) => _openLoans.Add(loanId);

    public bool Close(string loanId)
    {
        bool wasOpen = _openLoans.Remove(loanId);
        return wasOpen;
    }
}
