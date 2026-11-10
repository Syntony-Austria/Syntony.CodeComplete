namespace Library.Lending;

public interface ILoanStore
{
    IReadOnlyList<string> OverdueMembers();

    void MarkReminded(string member);
}

public interface IMailer
{
    void Send(string member, string text);
}

public sealed class Reminders(ILoanStore store, IMailer mailer)
{
    public int SendAll()
    {
        IReadOnlyList<string> members = store.OverdueMembers();
        foreach (string member in members)
        {
            mailer.Send(member, "Please return your book.");
            store.MarkReminded(member);
        }

        return members.Count;
    }
}

// The same behaviour after a refactoring: it asks the store more often.
public sealed class RemindersRefactored(ILoanStore store, IMailer mailer)
{
    public int SendAll()
    {
        int sent = 0;
        for (int i = 0; i < store.OverdueMembers().Count; i++)
        {
            string member = store.OverdueMembers()[i];
            mailer.Send(member, "Please return your book.");
            sent++;
        }

        foreach (string member in store.OverdueMembers())
        {
            store.MarkReminded(member);
        }

        return sent;
    }
}

// Stub: gives fixed answers.
public sealed class StubLoanStore(params string[] members) : ILoanStore
{
    public IReadOnlyList<string> OverdueMembers() => members;

    public void MarkReminded(string member)
    {
    }
}

// Spy: writes down what happened.
public sealed class SpyMailer : IMailer
{
    public List<string> Sent { get; } = [];

    public void Send(string member, string text) => Sent.Add(member);
}

// Fake: really works, only simpler than the real thing.
public sealed class FakeLoanStore(params string[] members) : ILoanStore
{
    private readonly List<string> _overdue = [.. members];

    public IReadOnlyList<string> OverdueMembers() => _overdue.ToArray();

    public void MarkReminded(string member) => Reminded.Add(member);

    public HashSet<string> Reminded { get; } = [];
}
