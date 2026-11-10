using System.Security.Cryptography;
using System.Text;

namespace Library.Lending;

// Same value and same key: same pseudonym. The key never sits in the log.
public sealed class Pseudonyms
{
    private readonly byte[] _key;

    public Pseudonyms(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length < 32)
        {
            throw new ArgumentException(
                "Use a key of at least 32 bytes.", nameof(key));
        }

        _key = key.ToArray();
    }

    public string For(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string normal = value.Trim().ToLowerInvariant();
        byte[] mac = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(normal));
        return "p-" + Convert.ToHexStringLower(mac.AsSpan(0, 8));
    }

    // For comparison only: a plain hash that anyone can recompute.
    public static string PlainHash(string value)
    {
        string normal = value.Trim().ToLowerInvariant();
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normal));
        return "h-" + Convert.ToHexStringLower(hash.AsSpan(0, 8));
    }
}
