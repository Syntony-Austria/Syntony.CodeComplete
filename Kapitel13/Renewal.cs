namespace Library.Lending;

public enum WaitingList
{
    Empty,
    SomeoneWaiting,
}

public enum MemberRank
{
    Regular,
    Prioritised,
}

public sealed record RenewalRequest(
    int RenewalsSoFar,
    DateOnly DueDate,
    DateOnly Today,
    WaitingList WaitingList,
    MemberRank Rank);

public static class RenewalPolicy
{
    public static bool IsFirstInQueue(RenewalRequest request) =>
        request.WaitingList == WaitingList.Empty
        || request.Rank == MemberRank.Prioritised;

    public static string Label(MemberRank rank) => rank switch
    {
        MemberRank.Regular => "regular",
        MemberRank.Prioritised => "prioritised",
        _ => throw new ArgumentOutOfRangeException(nameof(rank)),
    };
}
