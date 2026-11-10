using NUnit.Framework;

namespace Library.Lending;

public sealed class TwoReadingsTests
{
    [Test]
    public void BothReadingsAgreeDuringTheGrace([Range(0, 3)] int days)
    {
        Assert.That(TwoReadings.GraceDaysAreFree(days), Is.Zero);
        Assert.That(TwoReadings.GraceOnlyForgives(days), Is.Zero);
    }

    [TestCase(4, 25, 100)]
    [TestCase(5, 50, 125)]
    [TestCase(10, 175, 250)]
    public void FromTheFourthDayTheyDiffer(int days, int a, int b)
    {
        Assert.That(TwoReadings.GraceDaysAreFree(days), Is.EqualTo(a));
        Assert.That(TwoReadings.GraceOnlyForgives(days), Is.EqualTo(b));
    }

    // The gap is always the three grace days: 3 x 25 = 75 cents.
    [Test]
    public void TheGapIsAlways75Cents([Values(4, 5, 10, 100, 400)] int days)
    {
        Assert.That(TwoReadings.GraceOnlyForgives(days)
            - TwoReadings.GraceDaysAreFree(days), Is.EqualTo(75));
    }
}
