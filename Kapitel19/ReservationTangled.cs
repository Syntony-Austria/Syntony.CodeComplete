namespace Library.Lending;

public sealed class QueueFullException(string message) : Exception(message);

public sealed class ReservationDeskTangled(int capacity)
{
    private readonly List<string> _members = [];

    // A full queue is normal here, and yet it is an exception.
    public void Reserve(string memberId)
    {
        if (_members.Count >= capacity)
        {
            throw new QueueFullException("The queue is full.");
        }

        _members.Add(memberId);
    }

    // Deliberately wrong: it swallows the bug together with the expected case.
    public bool TryReserve(string? memberId)
    {
        try
        {
            Reserve(memberId!.Trim());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
