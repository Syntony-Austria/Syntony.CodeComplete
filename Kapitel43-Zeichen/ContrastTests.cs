using NUnit.Framework;

namespace Library.Lending;

public sealed class ContrastTests
{
    [Test]
    public void BlackOnWhiteIsTwentyOneToOne()
    {
        Assert.That(Contrast.Ratio("#000000", "#FFFFFF"),
            Is.EqualTo(21.0).Within(1e-9));
        Assert.That(Contrast.Ratio("#FFFFFF", "#000000"),
            Is.EqualTo(21.0).Within(1e-9));
        Assert.That(Contrast.Ratio("#8A5A2B", "#8A5A2B"), Is.EqualTo(1.0));
    }

    [TestCase("#767676", 4.54)]
    [TestCase("#777777", 4.48)]
    [TestCase("#9A9A9A", 2.81)]
    public void GreyOnWhite(string grey, double expected)
    {
        Assert.That(Contrast.Ratio(grey, "#FFFFFF"),
            Is.EqualTo(expected).Within(0.005));
    }

    [Test]
    public void EveryPairOfThePaletteMeetsItsMinimum()
    {
        Assert.That(LibraryPalette.Failures(LibraryPalette.Pairs), Is.Empty);
    }

    [Test]
    public void TheCheckNamesThePairAndTheRatio()
    {
        ColourPair pale = new("hint on paper", "#9A9A9A", "#FFFFFF", 4.5);

        Assert.That(LibraryPalette.Failures([pale]),
            Is.EqualTo(new[] { "hint on paper: 2.81:1, needs 4.5:1" }));
    }
}
