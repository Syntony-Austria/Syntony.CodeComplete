using System.Globalization;
using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run only with: dotnet test -p:Probe=true
// Each probe expects what intuition says; the measurement disagrees.
public sealed class ProbeTests
{
    private const string Box =
        "Feuerwehrb\u00FCcher \U0001F469\U0001F3FD\u200D\U0001F692";

    [Test]
    public void TheFirstDirectoryFindsTheDecomposedName()
    {
        RawMemberDirectory raw = new();
        raw.Add("m-1", "Mu\u0308ller");

        Assert.That(raw.Find("M\u00FCller"), Is.EqualTo("m-1"));
    }

    [Test]
    public void ANaiveCutKeepsTheTextWellFormed()
    {
        Assert.That(Text.IsWellFormed(Text.NaiveTruncate(Box, 19)), Is.True);
    }

    [Test]
    public void TheTurkishUpperCaseOfFileIsFile()
    {
        Assert.That("file".ToUpper(new CultureInfo("tr-TR")),
            Is.EqualTo("FILE"));
    }

    [Test]
    public void TheNullCharacterStandsAtThree()
    {
        Assert.That("Hel\0lo".IndexOf("\0", StringComparison.CurrentCulture),
            Is.EqualTo(3));
    }

    [Test]
    public void TheBrokenPairIsComplete()
    {
        Assert.That(ResxParity.Check(
            Paths.Source("Probe", "Broken", "Strings.resx"),
            Paths.Source("Probe", "Broken", "Strings.de.resx")), Is.Empty);
    }

    [Test]
    public void APaleHintPassesTheContrastCheck()
    {
        ColourPair pale = new("hint on paper", "#9A9A9A", "#FFFFFF", 4.5);

        Assert.That(LibraryPalette.Failures([.. LibraryPalette.Pairs, pale]),
            Is.Empty);
    }

    [Test]
    public void TheGoldenFileAcceptsAnAmericanFormat()
    {
        OverdueLine[] lines =
        [
            new("m-1", 5, new DateOnly(2026, 11, 3), 50),
            new("m-2", 16, new DateOnly(2026, 10, 23), 325),
        ];
        string screen = OverdueScreen.Render(lines,
            CultureInfo.GetCultureInfo("de"),
            CultureInfo.GetCultureInfo("en-US"));

        Assert.That(Golden.Compare(screen,
            Paths.Source("Golden", "overdue.de.de-AT.txt")), Is.Null);
    }
}
