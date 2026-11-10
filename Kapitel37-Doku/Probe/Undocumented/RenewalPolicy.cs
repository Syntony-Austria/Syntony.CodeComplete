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
    private const int MaxDaysOverdueForRenewal = 14;

    public static bool MayRenew(RenewalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.RenewalsSoFar < MaxRenewals
            && request.Today <= request.DueDate.AddDays(MaxDaysOverdueForRenewal)
            && (!request.HasWaitingReservation || request.IsPrioritisedMember);
    }
}

internal static class ReminderWindow
{
    public static int Days => 14;
}
