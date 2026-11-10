namespace Library.Lending;

public sealed record RenewalRequest(
    int RenewalsSoFar,
    DateOnly DueDate,
    DateOnly Today,
    bool HasWaitingReservation,
    bool IsPrioritisedMember);

public static class RenewalPolicy
{
    private const int MaxRenewals = 2;

    // Staff asked for a hard limit: after two weeks the book counts as missing
    // and can no longer be renewed.
    private const int MaxDaysOverdueForRenewal = 14;

    public static bool MayRenew(RenewalRequest request)
    {
        bool hasRenewalsLeft = request.RenewalsSoFar < MaxRenewals;
        bool isStillInRenewalWindow =
            request.Today <= request.DueDate.AddDays(MaxDaysOverdueForRenewal);
        bool isFirstInQueue =
            !request.HasWaitingReservation || request.IsPrioritisedMember;

        return hasRenewalsLeft && isStillInRenewalWindow && isFirstInQueue;
    }
}
