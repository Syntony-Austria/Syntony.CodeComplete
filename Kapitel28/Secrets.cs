using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Library.Lending;

public static class ResetToken
{
    // Deliberately wrong: anyone who knows the seed knows every token.
    public static string Tangled(int seed) =>
        new Random(seed).Next().ToString(CultureInfo.InvariantCulture);

    public static string Create() => RandomNumberGenerator.GetHexString(32);

    public static bool Matches(string expected, string given) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(given));
}

public static class Passwords
{
    private const int Iterations = 600_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltBytes);
        byte[] hash = Derive(password, salt);
        return Convert.ToHexString(salt) + ":" + Convert.ToHexString(hash);
    }

    public static bool Verify(string password, string stored)
    {
        string[] parts = stored.Split(':');
        byte[] actual = Derive(password, Convert.FromHexString(parts[0]));
        return CryptographicOperations.FixedTimeEquals(
            actual, Convert.FromHexString(parts[1]));
    }

    private static byte[] Derive(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(
            password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
}
