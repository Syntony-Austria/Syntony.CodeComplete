using System.Diagnostics.CodeAnalysis;

namespace Library.Lending;

public sealed class MemberNames
{
    private readonly Dictionary<string, string> _namesById =
        new(StringComparer.Ordinal);

    public void Add(string memberId, string name) => _namesById[memberId] = name;

    // Get: the member must exist. A missing one is a bug and throws.
    public string GetName(string memberId) => _namesById[memberId];

    // Find: the member may be missing. The caller has to handle null.
    public string? FindName(string memberId) =>
        _namesById.TryGetValue(memberId, out string? name) ? name : null;

    // Try: reports success, and the out value is safe to use after a true.
    public bool TryGetName(
        string memberId,
        [NotNullWhen(true)] out string? name) =>
        _namesById.TryGetValue(memberId, out name);
}
