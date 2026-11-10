using NUnit.Framework;

namespace Library.Lending;

public sealed class WipTests
{
    // The first 30 pieces of the demo year, their days as work units.
    private static readonly double[] s_sizes =
        [.. DemoHistory.Create().Take(30).Select(i => (double)i.Days)];

    [Test]
    public void ALimitOfThreeFinishesTheFirstPieceSooner()
    {
        WipResult three = WipSimulation.Run(s_sizes, people: 3, limit: 3);
        WipResult all = WipSimulation.Run(s_sizes, people: 3, limit: 30);

        Assert.That((three.FirstDone, all.FirstDone), Is.EqualTo((5.0, 20.0)));
        Assert.That(three.MedianDays, Is.EqualTo(6.0));
        Assert.That(all.MedianDays, Is.EqualTo(51.67).Within(0.01));
    }

    // Limit, first done, mean done, median cycle time, last done (days).
    [TestCase(3, 5.0, 38.83, 6.0, 97.0)]
    [TestCase(6, 4.0, 40.66, 12.0, 94.0)]
    [TestCase(12, 8.0, 45.10, 24.0, 92.67)]
    [TestCase(30, 20.0, 50.88, 51.67, 88.33)]
    public void TheTableInTheChapter(
        int limit, double first, double mean, double median, double last)
    {
        WipResult r = WipSimulation.Run(s_sizes, people: 3, limit);

        Assert.That(new[] { r.FirstDone, r.MeanDone, r.MedianDays, r.LastDone },
            Is.EqualTo(new[] { first, mean, median, last }).Within(0.01));
    }

    [Test]
    public void WithoutALimitTheLastPieceIsNotLater()
    {
        WipResult three = WipSimulation.Run(s_sizes, people: 3, limit: 3);
        WipResult all = WipSimulation.Run(s_sizes, people: 3, limit: 30);

        Assert.That(all.LastDone, Is.LessThan(three.LastDone));
    }
}
