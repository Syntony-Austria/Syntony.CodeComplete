using NUnit.Framework;

namespace Library.Lending;

public sealed class QueueTests
{
    [Test]
    public void TheSameSeedGivesTheSameRun()
    {
        Assert.That(QueueSimulation.Run(0.8, 10_000, seed: 42),
            Is.EqualTo(QueueSimulation.Run(0.8, 10_000, seed: 42)));
    }

    [TestCase(0.5)]
    [TestCase(0.8)]
    [TestCase(0.95)]
    public void LittlesLawHoldsInTheSimulation(double utilization)
    {
        QueueRun run = QueueSimulation.Run(utilization, 200_000, seed: 42);

        double lambdaW = run.ArrivalsPerMs * run.Response.Mean;

        Assert.That(run.SampledInSystem, Is.EqualTo(lambdaW).Within(1).Percent);
    }

    [Test]
    public void TheWaitGrowsSteeplyNearCapacity()
    {
        QueueRun half = QueueSimulation.Run(0.5, 200_000, seed: 42);
        QueueRun most = QueueSimulation.Run(0.8, 200_000, seed: 42);
        QueueRun full = QueueSimulation.Run(0.95, 200_000, seed: 42);

        Assert.That(most.MeanWait, Is.GreaterThan(half.MeanWait * 3));
        Assert.That(full.MeanWait, Is.GreaterThan(most.MeanWait * 4));
    }
}
