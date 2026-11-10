using System.Diagnostics.CodeAnalysis;

namespace Library.Lending;

public sealed record Isbn
{
    private Isbn(string digits) => Digits = digits;

    public string Digits { get; }

    public static bool TryParse(string text, [NotNullWhen(true)] out Isbn? isbn)
    {
        string digits = text.Replace("-", "", StringComparison.Ordinal);
        bool isValid = digits.Length == 13 && digits.All(char.IsAsciiDigit)
            && CheckSum(digits) % 10 == 0;

        isbn = isValid ? new Isbn(digits) : null;
        return isValid;
    }

    private static int CheckSum(string digits) =>
        digits
            .Select((digit, index) => (digit - '0') * (index % 2 == 0 ? 1 : 3))
            .Sum();
}
