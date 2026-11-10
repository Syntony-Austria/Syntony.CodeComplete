namespace Library.Lending;

// Deliberately wrong: three layers, one thought.
public sealed class LoanControllerTangled(LoanServiceTangled service)
{
    public int Count(string memberId) => service.Count(memberId);
}

public sealed class LoanServiceTangled(LoanRepositoryTangled repository)
{
    public int Count(string memberId) => repository.Count(memberId);
}

public sealed class LoanRepositoryTangled
{
    public int Count(string memberId) => memberId.Length;
}
