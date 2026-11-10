namespace Library.Lending;

public sealed class LoanHandoverTangled(List<string> waiting, List<string> lent)
{
    // Deliberately wrong: if the store fails, the member has lost the place.
    public void Hand(string memberId, Action<string> store)
    {
        waiting.Remove(memberId);
        store(memberId);
        lent.Add(memberId);
    }
}

public sealed class LoanHandover(List<string> waiting, List<string> lent)
{
    // The part that can fail comes first; the rest cannot fail.
    public void Hand(string memberId, Action<string> store)
    {
        store(memberId);
        waiting.Remove(memberId);
        lent.Add(memberId);
    }
}
