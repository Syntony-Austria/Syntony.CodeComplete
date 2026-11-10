namespace Library.Lending;

// A catalog that never answers. It honours the token, as HttpClient does.
public sealed class SilentCatalog : ICatalog
{
    public int Calls { get; private set; }

    public async Task<string?> FindTitleAsync(
        string isbn, CancellationToken cancellationToken)
    {
        Calls++;
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return null;
    }
}

// A catalog that ignores the token: a timeout cannot stop it.
public sealed class DeafCatalog : ICatalog
{
    public async Task<string?> FindTitleAsync(
        string isbn, CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, CancellationToken.None);
        return null;
    }
}

// The partner library books the order and then loses the first answers.
public sealed class PartnerLibraryFake(int answersToLose) : IPartnerLibrary
{
    private readonly Dictionary<string, string> _byKey = [];
    private int _lost;

    public List<string> Orders { get; } = [];

    public Task<string> OrderAsync(
        string isbn, string memberId, string? orderKey,
        CancellationToken cancellationToken)
    {
        if (orderKey is not null
            && _byKey.TryGetValue(orderKey, out string? known))
        {
            return Answer(known);
        }

        string id = $"order-{Orders.Count + 1}";
        Orders.Add(id);
        if (orderKey is not null)
        {
            _byKey[orderKey] = id;
        }

        return Answer(id);
    }

    private Task<string> Answer(string id)
    {
        if (_lost < answersToLose)
        {
            _lost++;
            return Task.FromException<string>(
                new TimeoutException("The answer was lost on the way."));
        }

        return Task.FromResult(id);
    }
}

// A remote call that fails with fixed codes, then succeeds.
public sealed class ScriptedRemote(params string[] failures)
{
    public int Calls { get; private set; }

    public Task<Result<string>> CallAsync(CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(Calls <= failures.Length
            ? Result<string>.Failure(new(failures[Calls - 1], "scripted"))
            : Result<string>.Success("ok"));
    }
}
