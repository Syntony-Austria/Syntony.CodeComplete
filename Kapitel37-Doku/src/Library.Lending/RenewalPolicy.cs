namespace Library.Lending;

/// <summary>What the desk knows when a member asks to renew a loan.</summary>
/// <param name="RenewalsSoFar">Renewals already granted for this loan.</param>
/// <param name="DueDate">The current due date of the loan.</param>
/// <param name="Today">The day of the request, in library time.</param>
/// <param name="HasWaitingReservation">Another member waits for the book.</param>
/// <param name="IsPrioritisedMember">The member may renew despite a waiting
/// reservation.</param>
public sealed record RenewalRequest(
    int RenewalsSoFar,
    DateOnly DueDate,
    DateOnly Today,
    bool HasWaitingReservation,
    bool IsPrioritisedMember);

/// <summary>Decides whether a loan may be renewed.</summary>
public static class RenewalPolicy
{
    private const int MaxRenewals = 2;

    // ADR-0003: after this limit the loan can no longer be renewed.
    private const int MaxDaysOverdueForRenewal = 14;

    /// <summary>Applies the three renewal rules of the library.</summary>
    /// <param name="request">The facts of the request.</param>
    /// <returns><see langword="true"/> if all three rules allow it.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="request"/> is <see langword="null"/>.</exception>
    public static bool MayRenew(RenewalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool hasRenewalsLeft = request.RenewalsSoFar < MaxRenewals;
        bool isStillInRenewalWindow =
            request.Today <= request.DueDate.AddDays(MaxDaysOverdueForRenewal);
        bool isFirstInQueue =
            !request.HasWaitingReservation || request.IsPrioritisedMember;

        return hasRenewalsLeft && isStillInRenewalWindow && isFirstInQueue;
    }
}
