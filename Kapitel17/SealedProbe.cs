namespace Library.Lending;

// Deliberately wrong. Built only with -p:SealedProbe=true.
public sealed class SpecialLoan : Loan
{
    public SpecialLoan()
        : base("m-1", new DateOnly(2026, 11, 2))
    {
    }
}
