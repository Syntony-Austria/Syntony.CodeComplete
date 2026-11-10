using System.Buffers;
using System.Globalization;
using System.Text;

namespace Library.Lending;

// Four honest answers to "how long is this text?"
public sealed record TextLength(int Utf8Bytes, int Utf16Units, int Runes,
    int Graphemes);

public static class Text
{
    public static TextLength Measure(string text) => new(
        Encoding.UTF8.GetByteCount(text),
        text.Length,
        text.EnumerateRunes().Count(),
        new StringInfo(text).LengthInTextElements);

    // Cuts after whole user-perceived characters (grapheme clusters).
    public static string Truncate(string text, int maxGraphemes)
    {
        StringInfo info = new(text);
        if (info.LengthInTextElements <= maxGraphemes)
        {
            return text;
        }

        return info.SubstringByTextElements(0, maxGraphemes);
    }

    // What many programs do: count UTF-16 units.
    public static string NaiveTruncate(string text, int maxUnits) =>
        text.Length <= maxUnits ? text : text[..maxUnits];

    // Well-formed UTF-16: every surrogate stands next to its partner.
    public static bool IsWellFormed(string text)
    {
        ReadOnlySpan<char> rest = text;
        while (!rest.IsEmpty)
        {
            OperationStatus status =
                Rune.DecodeFromUtf16(rest, out _, out int used);
            if (status != OperationStatus.Done)
            {
                return false;
            }

            rest = rest[used..];
        }

        return true;
    }
}
