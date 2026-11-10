namespace Library.Lending;

public sealed class ReservationQueue
{
    private readonly List<string> _waitingMembers = [];

    public int WaitingCount => _waitingMembers.Count;

    public void Join(string memberId) => _waitingMembers.Add(memberId);

    public void Leave(string memberId) => _waitingMembers.Remove(memberId);
}
