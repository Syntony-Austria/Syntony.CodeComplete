namespace Library.Lending;

public sealed record TestLoan(
    string Member, MemberKind Kind, bool IsReference, int DaysLate);

// Every test names only what matters to it. The rest is a sensible default.
public sealed class LoanBuilder
{
    private string _member = "m-1";
    private MemberKind _kind = MemberKind.Adult;
    private bool _isReference;
    private int _daysLate;

    public LoanBuilder ForChild()
    {
        _kind = MemberKind.Child;
        return this;
    }

    public LoanBuilder Reference()
    {
        _isReference = true;
        return this;
    }

    public LoanBuilder Late(int days)
    {
        _daysLate = days;
        return this;
    }

    public TestLoan Build() => new(_member, _kind, _isReference, _daysLate);
}
