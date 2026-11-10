using NUnit.Framework;

namespace Library.Lending;

public sealed class CanaryTests
{
    public static readonly CanaryPlan Staged = new([1, 5, 25, 100], 200, 0.01);

    [Test]
    public void TheSameSeedGivesTheSameRun()
    {
        Assert.That(Canary.Run(Staged, 0.005, 0.03, 40),
            Is.EqualTo(Canary.Run(Staged, 0.005, 0.03, 40)));
    }

    [Test]
    public void ABadReleaseStopsAtOnePercent()
    {
        CanaryRun run = Canary.Run(Staged, 0.005, 0.03, 40);

        Assert.That(run.Completed, Is.False);
        Assert.That(run.LastPercent, Is.EqualTo(1));
        Assert.That(run.OnNew, Is.EqualTo(200));
    }

    [Test]
    public void AGoodReleaseGoesAllTheWay()
    {
        CanaryRun run = Canary.Run(Staged, 0.005, 0.005, 40);

        Assert.That(run.Completed, Is.True);
        Assert.That(run.LastPercent, Is.EqualTo(100));
    }

    [Test]
    public void PrintTheTable()
    {
        foreach (int minOnNew in new[] { 20, 200, 1000 })
        {
            CanaryPlan plan = Staged with { MinOnNew = minOnNew };
            CanaryRun[] bad = Runs(plan, 0.03);
            CanaryRun[] good = Runs(plan, 0.005);
            TestContext.Out.WriteLine(
                $"min {minOnNew}: bad stopped {bad.Count(r => !r.Completed)}"
                + $", at 1% {bad.Count(r => r.LastPercent == 1)}"
                + $", failed on new {Mean(bad, r => r.FailedOnNew):F1}"
                + $", requests {Mean(bad, r => r.Requests):F0}"
                + $", all-at-once failures {Mean(bad, AllAtOnce):F1}"
                + $"; good stopped {good.Count(r => !r.Completed)}");
        }
    }

    private static CanaryRun[] Runs(CanaryPlan plan, double newRate) =>
        [.. Enumerable.Range(1, 100)
            .Select(seed => Canary.Run(plan, 0.005, newRate, seed))];

    private static double Mean(CanaryRun[] runs, Func<CanaryRun, int> pick) =>
        runs.Average(pick);

    // The same number of requests, all on the new version at once.
    private static int AllAtOnce(CanaryRun run) =>
        Canary.Run(new([100], run.Requests, 0.01), 0.005, 0.03, 1).FailedOnNew;
}
