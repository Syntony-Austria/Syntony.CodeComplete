namespace Library.Lending;

// The record from chapter 2 (Listing 8) breaks the rule derived from it.
// dotnet build -p:RecordProbe=true -> LIB001 at IsPrioritisedMember.
public sealed record RenewalRequest(
    int RenewalsSoFar,
    DateOnly DueDate,
    DateOnly Today,
    bool HasWaitingReservation,
    bool IsPrioritisedMember);
