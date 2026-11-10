using NUnit.Framework;

namespace Library.Lending;

public sealed class TailTests
{
    [TestCase(1, 0.010)]
    [TestCase(10, 0.096)]
    [TestCase(30, 0.260)]
    [TestCase(100, 0.634)]
    public void OneSlowInAHundredHitsMostWideRequests(int fanOut, double any)
    {
        Assert.That(Tail.AnySlow(0.01, fanOut), Is.EqualTo(any).Within(0.0005));
    }

    [Test]
    public void TheSimulationAgreesWithTheFormula()
    {
        double[] pool = QueueSimulation.Responses(0.9, 200_000, seed: 42);
        double p99 = Latency.Summarize(pool).P99;

        double[] lists = FanOut.Slowest(pool, 100, 20_000, seed: 7);
        double slower = (double)lists.Count(t => t > p99) / lists.Length;

        Assert.That(slower, Is.EqualTo(Tail.AnySlow(0.01, 100)).Within(0.01));
    }

    [Test]
    public void TheP99OfOneLookupBecomesTheMedianOfSeventy()
    {
        double[] pool = QueueSimulation.Responses(0.9, 200_000, seed: 42);
        double p99 = Latency.Summarize(pool).P99;

        double[] lists = FanOut.Slowest(pool, 70, 20_000, seed: 7);
        double median = Latency.Summarize(lists).P50;

        Assert.That(median, Is.EqualTo(p99).Within(2).Percent);
    }
}
