using NUnit.Framework;

namespace Library.Lending;

public sealed class BudgetTests
{
    [Test]
    public void NinetyNinePointNineInThirtyDaysIsAboutFortyThreeMinutes()
    {
        TimeSpan allowed = ErrorBudget.Allowed(0.999, TimeSpan.FromDays(30));

        Assert.That(allowed.TotalMinutes, Is.EqualTo(43.2).Within(0.001));
    }

    [Test]
    public void AFullBudgetIsOneAndAnOverdrawnOneIsNegative()
    {
        Assert.That(ErrorBudget.Left(0.99, good: 1000, total: 1000),
            Is.EqualTo(1));
        Assert.That(ErrorBudget.Left(0.99, good: 995, total: 1000),
            Is.EqualTo(0.5).Within(1e-9));
        Assert.That(ErrorBudget.Left(0.99, good: 970, total: 1000),
            Is.EqualTo(-2).Within(1e-9));
    }

    [TestCase(0.80, true)]
    [TestCase(0.90, true)]
    [TestCase(0.95, false)]
    public void TheGoalShowsWhichMorningMissesIt(double load, bool met)
    {
        double[] times = QueueSimulation.Responses(load, 200_000, seed: 42);

        Assert.That(ErrorBudget.Sli(times, 500) >= 0.99, Is.EqualTo(met));
    }
}
