using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Library.Lending;

public sealed record SigningKey(string Id, byte[] Secret);

public sealed record RenewalLink(string MemberId, string LoanId,
    DateTimeOffset ExpiresAt);

// Several keys, found by id. New links use the active one; old links
// keep working as long as their key is still in the ring.
public sealed class KeyRing
{
    private readonly Dictionary<string, byte[]> _keys;

    public KeyRing(IEnumerable<SigningKey> keys, string activeId)
    {
        _keys = keys.ToDictionary(key => key.Id, key => key.Secret);
        if (!_keys.ContainsKey(activeId))
        {
            throw new ArgumentException($"No key '{activeId}'.",
                nameof(activeId));
        }

        ActiveId = activeId;
    }

    public string ActiveId { get; }

    public byte[]? Find(string id) => _keys.GetValueOrDefault(id);
}

// Token: keyId.payload.signature, the signature covers keyId and payload.
public static class RenewalLinks
{
    public static string Sign(RenewalLink link, KeyRing ring)
    {
        string payload = string.Join('|', link.MemberId, link.LoanId,
            link.ExpiresAt.ToUnixTimeSeconds());
        string body = ring.ActiveId + "." + Encode(payload);
        byte[] secret = ring.Find(ring.ActiveId)!;
        return body + "." + Base64Url.EncodeToString(Mac(secret, body));
    }

    public static Result<RenewalLink> Verify(string token, KeyRing ring,
        TimeProvider clock)
    {
        string[] parts = token.Split('.');
        if (parts.Length != 3)
        {
            return Fail("link.malformed", "Expected three parts.");
        }

        byte[]? secret = ring.Find(parts[0]);
        if (secret is null)
        {
            return Fail("link.unknown_key", $"Key '{parts[0]}' is not known.");
        }

        byte[] expected = Mac(secret, parts[0] + "." + parts[1]);
        if (!Base64Url.IsValid(parts[2], out _) ||
            !CryptographicOperations.FixedTimeEquals(expected,
                Base64Url.DecodeFromChars(parts[2])))
        {
            return Fail("link.bad_signature", "The signature does not match.");
        }

        string[] fields = Encoding.UTF8
            .GetString(Base64Url.DecodeFromChars(parts[1])).Split('|');
        DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeSeconds(
            long.Parse(fields[2], CultureInfo.InvariantCulture));
        return clock.GetUtcNow() >= expiresAt
            ? Fail("link.expired", $"The link expired at {expiresAt:u}.")
            : Result<RenewalLink>.Success(
                new RenewalLink(fields[0], fields[1], expiresAt));
    }

    private static string Encode(string text) =>
        Base64Url.EncodeToString(Encoding.UTF8.GetBytes(text));

    private static byte[] Mac(byte[] secret, string body) =>
        HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes(body));

    private static Result<RenewalLink> Fail(string code, string message) =>
        Result<RenewalLink>.Failure(code, message);
}
