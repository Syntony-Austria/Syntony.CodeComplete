using NUnit.Framework;

namespace Library.Lending;

public sealed class LatencyTests
{
    [Test]
    public void NearestRankReturnsATimeThatReallyOccurred()
    {
        double[] sorted = [10, 20, 30, 40];

        Assert.That(Latency.Percentile(sorted, 50), Is.EqualTo(20));
        Assert.That(Latency.Percentile(sorted, 99), Is.EqualTo(40));
    }

    [Test]
    public void PercentilesOfTwoCountersDoNotAverage()
    {
        double[] quiet = QueueSimulation.Responses(0.5, 200_000, seed: 42);
        double[] busy = QueueSimulation.Responses(0.95, 200_000, seed: 42);
        double averaged =
            (Latency.Summarize(quiet).P99 + Latency.Summarize(busy).P99) / 2;

        double merged = Latency.Summarize(quiet.Concat(busy)).P99;

        Assert.That(merged, Is.GreaterThan(averaged * 1.4));
    }
}
