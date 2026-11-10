namespace Library.Lending;

public enum Priority
{
    Normal,
    Prioritised,
}

public static class Renewals
{
    public static bool MayRenew(bool hasReservation, Priority priority) =>
        !hasReservation || priority == Priority.Prioritised;
}
