using JetBrains.Annotations;

namespace Library.Lending;

public sealed record Reservation(string MemberId, int Position);

public sealed class ReservationDesk(int capacity)
{
    public static readonly Error QueueFull =
        new("reservation.queue_full", "The queue for this book is full.");

    public static readonly Error AlreadyWaiting =
        new("reservation.already_waiting", "This member is already waiting.");

    private readonly List<string> _members = [];

    [MustUseReturnValue]
    public Result<Reservation> Reserve(string memberId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);

        if (_members.Contains(memberId))
        {
            return Result<Reservation>.Failure(AlreadyWaiting);
        }

        if (_members.Count >= capacity)
        {
            return Result<Reservation>.Failure(QueueFull);
        }

        _members.Add(memberId);
        return Result<Reservation>.Success(new(memberId, _members.Count));
    }
}
