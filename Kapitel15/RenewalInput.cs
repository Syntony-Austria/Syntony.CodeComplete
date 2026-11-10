namespace Library.Lending;

public sealed record RenewalInput(
    int DaysOverdue,
    int RenewalsSoFar,
    bool HasOpenFee,
    bool SomeoneWaiting,
    bool IsPrioritised);
