using NUnit.Framework;

namespace Library.Lending;

public sealed class ForecastTests
{
    private static readonly List<WorkItem> s_items = DemoHistory.Create();

    private static int[] Weekly => Flow.WeeklyThroughput(
        s_items, DemoHistory.FirstMonday, DemoHistory.Weeks);

    [Test]
    public void TheCsvFileIsTheDemoHistory()
    {
        Assert.That(File.ReadAllLines(Paths.Data("done.csv")).Skip(1)
            .Select(WorkItem.FromCsv), Is.EqualTo(s_items));
    }

    [Test]
    public void TheYearHas157FinishedPiecesAndFourEmptyWeeks()
    {
        Assert.That(Weekly.Sum(), Is.EqualTo(157));
        Assert.That(Weekly.Count(w => w == 0), Is.EqualTo(4));
        Assert.That(Weekly[0], Is.Zero, "the first week is the start week");
        Assert.That(Weekly[^12..].Average(), Is.EqualTo(3.25));
    }

    [Test]
    public void ThirtyPiecesNeedTenToTwelveWeeks()
    {
        int[] sim = Forecast.MonteCarlo(Weekly[^12..], 30, 10_000, seed: 1);

        Assert.That(Plan.ByAverage(Weekly[^12..], 30), Is.EqualTo(10));
        Assert.That(new[]
        {
            Forecast.Percentile(sim, 0.50), Forecast.Percentile(sim, 0.85),
            Forecast.Percentile(sim, 0.95),
        }, Is.EqualTo(new[] { 10, 11, 12 }));
        Assert.That((sim[0], sim[^1]), Is.EqualTo((6, 14)));
    }

    [Test]
    public void FiveFewerPiecesMoveThe85MarkToWeekNine()
    {
        int[] sim = Forecast.MonteCarlo(Weekly[^12..], 25, 10_000, seed: 1);

        Assert.That(Forecast.Percentile(sim, 0.85), Is.EqualTo(9));
    }

    [Test]
    public void TheSameSeedGivesTheSameRuns()
    {
        Assert.That(Forecast.MonteCarlo(Weekly[^12..], 30, 1_000, seed: 7),
            Is.EqualTo(Forecast.MonteCarlo(Weekly[^12..], 30, 1_000, seed: 7)));
    }

    [Test]
    public void WeeksWithoutAFinishedPieceCannotForecast()
    {
        Assert.That(() => Forecast.MonteCarlo([0, 0, 0], 30, 100, seed: 1),
            Throws.ArgumentException);
    }
}
