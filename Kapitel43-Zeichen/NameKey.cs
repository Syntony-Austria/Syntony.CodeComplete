using System.Text;

namespace Library.Lending;

// The wall for names: one form inside, NFC (Unicode normalization form C).
public static class NameKey
{
    // In invariant globalization mode Normalize() may do nothing and
    // IsNormalized() may say "true". Check once instead of trusting it.
    public static bool NormalizationWorks { get; } =
        "u\u0308".Normalize(NormalizationForm.FormC) == "\u00FC";

    public static string? TryCreate(string raw)
    {
        if (!NormalizationWorks)
        {
            return null; // fail closed: no key we cannot trust
        }

        string key = raw.Trim().Normalize(NormalizationForm.FormC);
        return key.Length == 0 ? null : key;
    }
}

public sealed class MemberDirectory
{
    private readonly Dictionary<string, string> _ids =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add(string memberId, string name)
    {
        if (!NameKey.NormalizationWorks)
        {
            // Name the cause: the environment, not the name.
            throw new InvalidOperationException(
                "Unicode normalization is not available (invariant mode).");
        }

        _ids[NameKey.TryCreate(name)
            ?? throw new ArgumentException("No usable name.", nameof(name))]
            = memberId;
    }

    public string? Find(string name) =>
        NameKey.TryCreate(name) is string key
            && _ids.TryGetValue(key, out string? id) ? id : null;
}

// The first version: stores and searches the text as it came in.
public sealed class RawMemberDirectory
{
    private readonly Dictionary<string, string> _ids =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add(string memberId, string name) => _ids[name] = memberId;

    public string? Find(string name) =>
        _ids.TryGetValue(name, out string? id) ? id : null;
}
