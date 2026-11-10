using System.Globalization;
using NUnit.Framework;

namespace Library.Lending;

public sealed class ScreenTests
{
    private static readonly OverdueLine[] Lines =
    [
        new("m-1", 5, new DateOnly(2026, 11, 3), 50),
        new("m-2", 16, new DateOnly(2026, 10, 23), 325),
    ];

    private static CultureInfo C(string name) =>
        CultureInfo.GetCultureInfo(name);

    [Test]
    public void TheGermanScreenMatchesItsGoldenFile()
    {
        string screen = OverdueScreen.Render(Lines, C("de"), C("de-AT"));

        Assert.That(Golden.Compare(screen,
            Paths.Source("Golden", "overdue.de.de-AT.txt")), Is.Null);
    }

    [Test]
    public void AGoldenFileWithWindowsLineEndsStillMatches()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "Overdue loans\r\nm-1\r\n");

            Assert.That(Golden.Compare("Overdue loans\nm-1\n", path), Is.Null);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void LanguageAndFormatAreTwoSettings()
    {
        string screen = OverdueScreen.Render(Lines, C("en"), C("de-AT"));

        Assert.That(screen, Does.StartWith("Overdue loans\n"));
        Assert.That(screen, Does.Contain("due 03.11.2026, fee \u20AC 0,50"));
    }

    [Test]
    public void TheGermanTextReachesTheProgram()
    {
        // Fails if the satellite assembly de/*.resources.dll is missing.
        Assert.That(OverdueScreen.Text("Overdue.Title", C("de-AT")),
            Is.EqualTo("\u00DCberf\u00E4llige Ausleihen"));
        Assert.That(File.Exists(Path.Combine(AppContext.BaseDirectory, "de",
            "Kapitel43-Zeichen.resources.dll")), Is.True);
    }

    [TestCase("Strings.resx", "Strings.de.resx")]
    [TestCase("Strings.de.resx", "Strings.resx")]
    public void EveryTextExistsInBothLanguages(string from, string to)
    {
        Assert.That(ResxParity.Check(Paths.Source("Resources", from),
            Paths.Source("Resources", to)), Is.Empty);
    }

    [Test]
    public void TheParityCheckFindsABrokenPair()
    {
        IReadOnlyList<string> problems = ResxParity.Check(
            Paths.Source("Probe", "Broken", "Strings.resx"),
            Paths.Source("Probe", "Broken", "Strings.de.resx"));

        Assert.That(problems, Is.EquivalentTo(new[]
        {
            "missing in translation: Overdue.Total",
            "placeholders differ in Overdue.Line: {0,1,2,3} vs {0,1,3}",
        }));
    }

    [Test]
    public void AMissingPlaceholderDropsAValueSilently()
    {
        string line = string.Format(C("de-AT"),
            "{0}: {1} Tage, Geb\u00FChr {3:C}",
            "m-1", 5, new DateOnly(2026, 11, 3), 0.50m);

        Assert.That(line, Is.EqualTo("m-1: 5 Tage, Geb\u00FChr \u20AC 0,50"));
    }
}
