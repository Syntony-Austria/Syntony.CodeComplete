using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run alone:
// dotnet test -p:Probe=true --filter Category=Probe
[Category("Probe")]
public sealed class ProbeTests
{
    private static readonly List<WorkItem> s_items = DemoHistory.Create();

    // The plan by average is expected to be as safe as the 85 % mark.
    [Test]
    public void TheAverageHoldsLikeThe85Mark()
    {
        int[] weekly = Flow.WeeklyThroughput(
            s_items, DemoHistory.FirstMonday, DemoHistory.Weeks);
        List<Trial> trials = Backtest.Run(weekly, 30, 12, 10_000, seed: 1);

        Assert.That(Backtest.HitRate(trials, t => t.ByAverage),
            Is.GreaterThanOrEqualTo(0.85));
    }

    // Little's Law is expected to hold in a window that is not stationary.
    [Test]
    public void LittleHoldsInTheSummerWeeks()
    {
        DateOnly summer = DemoHistory.FirstMonday.AddDays(29 * 7);
        LittleCheck c = Flow.Little(s_items, summer, summer.AddDays(21));

        Assert.That(c.Product, Is.EqualTo(c.MeanWip).Within(5).Percent);
    }

    // Without a WIP limit the first piece is expected just as early.
    [Test]
    public void WithoutALimitTheFirstPieceIsJustAsEarly()
    {
        double[] sizes = [.. s_items.Take(30).Select(i => (double)i.Days)];

        Assert.That(WipSimulation.Run(sizes, 3, 30).FirstDone,
            Is.EqualTo(WipSimulation.Run(sizes, 3, 3).FirstDone));
    }

    // Big pieces are expected to show progress as often as small ones.
    [Test]
    public void BigPiecesShowProgressAsOften()
    {
        int[] small =
            Flow.WeeklyThroughput(s_items, DemoHistory.FirstMonday, 52);
        int[] big = Flow.WeeklyThroughput(
            Flow.Merge(s_items, 3), DemoHistory.FirstMonday, 52);

        Assert.That(big.Count(w => w == 0),
            Is.EqualTo(small.Count(w => w == 0)));
    }
}
