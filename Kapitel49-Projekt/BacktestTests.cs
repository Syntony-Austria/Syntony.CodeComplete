using NUnit.Framework;

namespace Library.Lending;

public sealed class BacktestTests
{
    [Test]
    public void InTheDemoYearTheAverageIsLateInTenOf31Trials()
    {
        int[] weekly = Flow.WeeklyThroughput(
            DemoHistory.Create(), DemoHistory.FirstMonday, DemoHistory.Weeks);
        List<Trial> trials = Backtest.Run(weekly, 30, 12, 10_000, seed: 1);

        Assert.That(trials, Has.Count.EqualTo(31));
        Assert.That(trials.Count(t => t.Actual > t.ByAverage), Is.EqualTo(10));
        Assert.That(trials.Count(t => t.Actual > t.P50), Is.EqualTo(10));
        Assert.That(trials.Count(t => t.Actual > t.P85), Is.Zero);
        Assert.That(trials.Count(t => t.Actual > t.P95), Is.Zero);
    }

    [Test]
    public void OverFiftyYearsThe85MarkHoldsInFourOfFive()
    {
        List<Trial> trials = [];
        for (int year = 1; year <= 50; year++)
        {
            int[] weekly = Flow.WeeklyThroughput(DemoHistory.Create(year),
                DemoHistory.FirstMonday, DemoHistory.Weeks);
            trials.AddRange(Backtest.Run(weekly, 30, 12, 2_000, seed: 1));
        }

        Assert.That(trials, Has.Count.EqualTo(1529));
        Assert.That(Math.Round(Backtest.HitRate(trials, t => t.ByAverage), 3),
            Is.EqualTo(0.560));
        Assert.That(Math.Round(Backtest.HitRate(trials, t => t.P50), 3),
            Is.EqualTo(0.556));
        Assert.That(Math.Round(Backtest.HitRate(trials, t => t.P85), 3),
            Is.EqualTo(0.788));
        Assert.That(Math.Round(Backtest.HitRate(trials, t => t.P95), 3),
            Is.EqualTo(0.874));
    }
}
