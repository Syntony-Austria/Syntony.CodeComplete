namespace Library.Lending;

// The chance comes in from outside. A test passes a seed.
public sealed class Raffle(Random random)
{
    public string Winner(IReadOnlyList<string> members) =>
        members[random.Next(members.Count)];
}
