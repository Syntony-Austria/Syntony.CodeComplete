using System.Globalization;
using NUnit.Framework;

namespace Library.Lending;

public sealed class ComparisonTests
{
    private static readonly CultureInfo Turkish = new("tr-TR");
    private static readonly CultureInfo Austrian = new("de-AT");

    [Test]
    public void TheTurkishUpperCaseOfFileIsNotFile()
    {
        Assert.That("file".ToUpper(Turkish), Is.EqualTo("F\u0130LE"));
        Assert.That("FILE".ToLower(Turkish), Is.EqualTo("f\u0131le"));
        Assert.That("file".ToUpperInvariant(), Is.EqualTo("FILE"));
    }

    [Test]
    public void IgnoringCaseDependsOnWhoIgnores()
    {
        Assert.That(string.Equals("file", "FILE",
            StringComparison.OrdinalIgnoreCase), Is.True);
        Assert.That(string.Compare("file", "FILE", Austrian,
            CompareOptions.IgnoreCase), Is.Zero);
        Assert.That(string.Compare("file", "FILE", Turkish,
            CompareOptions.IgnoreCase), Is.Not.Zero);
    }

    [Test]
    public void ALinguisticSearchFindsTheNullCharacterAtZero()
    {
        // IndexOf(string) compares by culture; ICU ignores "\0" here.
        Assert.That("Hel\0lo".IndexOf("\0", StringComparison.CurrentCulture),
            Is.Zero);
        Assert.That("Hel\0lo".IndexOf("\0", StringComparison.Ordinal),
            Is.EqualTo(3));
        Assert.That("Hel\0lo".IndexOf('\0'), Is.EqualTo(3)); // char: ordinal
    }

    [Test]
    public void ALinguisticComparisonSeesBothFormsOfAnUmlautAsEqual()
    {
        Assert.That(string.Equals("M\u00FCller", "Mu\u0308ller",
            StringComparison.InvariantCulture), Is.True);
        Assert.That(string.Equals("M\u00FCller", "Mu\u0308ller",
            StringComparison.Ordinal), Is.False);
    }

    [Test]
    public void SortingOrderIsACultureQuestion()
    {
        Assert.That(string.Compare("\u00C4pfel", "Zebra", Austrian,
            CompareOptions.None), Is.Negative);
        Assert.That(string.CompareOrdinal("\u00C4pfel", "Zebra"),
            Is.Positive);
    }
}
