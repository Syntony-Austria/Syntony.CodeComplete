namespace Library.Lending;

public sealed class LoanLimit(int limit)
{
    private readonly Lock _gate = new();
    private int _loans;

    public int Loans => _loans;

    // Deliberately wrong: between the check and the act, anything can happen.
    public bool TryLendTangled(Action? between = null)
    {
        if (_loans < limit)
        {
            between?.Invoke();
            _loans++;
            return true;
        }

        return false;
    }

    // Check and act are one step. The invariant: _loans never exceeds limit.
    public bool TryLend()
    {
        lock (_gate)
        {
            if (_loans >= limit)
            {
                return false;
            }

            _loans++;
            return true;
        }
    }
}
