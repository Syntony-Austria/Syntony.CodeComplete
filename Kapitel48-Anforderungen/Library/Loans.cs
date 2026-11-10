namespace Library.Lending;

// The member kind is taken when the book is lent (LEND-006).
public sealed record Loan(
    string Card, MemberKind Kind, bool Reference, DateOnly Due);

public sealed record Charge(string Card, int Cents);

public sealed class LoanDesk(TimeProvider clock)
{
    public const int LoanDays = 28;  // LEND-001

    private DateOnly Today =>
        DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    public Loan Lend(string card, MemberKind kind, bool reference) =>
        new(card, kind, reference, Today.AddDays(LoanDays));

    // LEND-009: the card on the loan pays, whoever brings the book back.
    public Charge Return(Loan loan, string returnedBy)
    {
        int daysLate = Math.Max(0, Today.DayNumber - loan.Due.DayNumber);
        return new Charge(
            loan.Card, LoanFee.InCents(daysLate, loan.Kind, loan.Reference));
    }
}
