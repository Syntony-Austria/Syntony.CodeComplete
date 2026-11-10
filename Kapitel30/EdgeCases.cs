using System.Globalization;
using System.Text;
using NUnit.Framework;

namespace Library.Lending;

// Inputs that look harmless and are not.
[TestFixture]
public sealed class EdgeCaseTests
{
    [Test]
    public void TwoSpellingsOfTheSameNameAreDifferentStrings()
    {
        string composed = "M\u00fcller";
        string decomposed = "Mu\u0308ller";

        Assert.Multiple(() =>
        {
            Assert.That(composed == decomposed, Is.False);
            Assert.That(composed.Normalize() == decomposed.Normalize(), Is.True);
        });
    }

    [Test]
    public void ASingleVisibleSignCanHaveLengthTwo()
    {
        Assert.Multiple(() =>
        {
            Assert.That("\U0001F4DA".Length, Is.EqualTo(2));
            Assert.That("\U0001F4DA".EnumerateRunes().Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void UpperCaseDependsOnTheCulture()
    {
        CultureInfo turkish = CultureInfo.GetCultureInfo("tr-TR");

        Assert.Multiple(() =>
        {
            Assert.That("i".ToUpper(turkish), Is.EqualTo("\u0130"));
            Assert.That("i".ToUpperInvariant(), Is.EqualTo("I"));
        });
    }

    [Test]
    public void AMonthForwardAndBackDoesNotReturn()
    {
        DateOnly start = new(2026, 1, 31);

        DateOnly there = start.AddMonths(1);

        Assert.Multiple(() =>
        {
            Assert.That(there, Is.EqualTo(new DateOnly(2026, 2, 28)));
            Assert.That(there.AddMonths(-1), Is.EqualTo(new DateOnly(2026, 1, 28)));
        });
    }

    [Test]
    public void RoundingGoesToTheEvenNeighbourByDefault()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Math.Round(0.125m, 2), Is.EqualTo(0.12m));
            Assert.That(Math.Round(0.135m, 2), Is.EqualTo(0.14m));
            Assert.That(
                Math.Round(0.125m, 2, MidpointRounding.AwayFromZero),
                Is.EqualTo(0.13m));
        });
    }

    // The rule says "half". It does not say what half of 125 cents is.
    [Test]
    public void HalfOfAnOddFeeLosesACent()
    {
        Assert.That(OverdueFee.For(5, MemberKind.Child, false), Is.EqualTo(62));
    }
}
