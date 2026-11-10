using System.Diagnostics;

namespace Library.Lending;

public sealed class LoanLedger
{
    private readonly List<LoanRequest> _requests = [];

    public int Count => _requests.Count;

    // Public: a wrong call is a bug in somebody else's code, so it throws always.
    public void Add(LoanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(request.Copies, 5);
        Store(request);
    }

    // Private: the data is inside the wall. An assertion documents the belief.
    private void Store(LoanRequest request)
    {
        Debug.Assert(request.MemberId.Length > 0, "The parser refuses empty ids.");
        _requests.Add(request);
    }
}
