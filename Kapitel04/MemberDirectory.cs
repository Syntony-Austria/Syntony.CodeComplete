namespace Library.Lending;

public sealed class MemberDirectory
{
    private readonly Dictionary<string, string> _namesById =
        new(StringComparer.Ordinal);

    public void Add(string memberId, string name) => _namesById[memberId] = name;

    public string? FindName(string memberId) =>
        _namesById.TryGetValue(memberId, out string? name) ? name : null;

    public int NameLength(string memberId)
    {
        string? name = FindName(memberId);
        return name is null ? 0 : name.Length;
    }
}
