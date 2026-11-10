using NUnit.Framework;

namespace Library.Lending;

public sealed class FlowTests
{
    private static readonly List<WorkItem> s_items = DemoHistory.Create();
    private static readonly DateOnly s_monday = DemoHistory.FirstMonday;

    [Test]
    public void LittleHoldsExactlyWhenTheWindowStartsAndEndsEmpty()
    {
        DateOnly after = s_items.Max(i => i.Finished).AddDays(1);
        LittleCheck c = Flow.Little(s_items, s_monday, after);

        Assert.That(c.Product, Is.EqualTo(c.MeanWip).Within(1e-9));
        Assert.That(c.MeanDays, Is.EqualTo(10.21).Within(0.005));
    }

    [Test]
    public void OverTheYearLittleMissesByOnePercent()
    {
        LittleCheck c = Flow.Little(s_items, s_monday, s_monday.AddDays(364));

        Assert.That(c.Product / c.MeanWip, Is.EqualTo(0.99).Within(0.005));
    }

    [Test]
    public void InTheSummerWeeksLittleMissesByMoreThanHalf()
    {
        DateOnly summer = s_monday.AddDays(29 * 7);
        DateOnly after = summer.AddDays(21);
        LittleCheck c = Flow.Little(s_items, summer, after);

        Assert.That(c.Product / c.MeanWip, Is.GreaterThan(1.5));
        Assert.That(c.MeanDays, Is.EqualTo(16.25));
        Assert.That(s_items.Count(i => i.Started >= summer && i.Started < after),
            Is.EqualTo(4));
        Assert.That(s_items.Count(i => i.Finished >= summer && i.Finished < after),
            Is.EqualTo(8));
    }

    [Test]
    public void BigPiecesLeaveMoreEmptyWeeks()
    {
        int[] small = Flow.WeeklyThroughput(s_items, s_monday, 52);
        int[] big = Flow.WeeklyThroughput(Flow.Merge(s_items, 3), s_monday, 52);

        Assert.That((small.Count(w => w == 0), big.Count(w => w == 0)),
            Is.EqualTo((4, 14)));
    }

    [Test]
    public void TenBigPiecesHaveAWiderForecastThanThirtySmallOnes()
    {
        int[] big = Flow.WeeklyThroughput(Flow.Merge(s_items, 3), s_monday, 52);
        int[] sim = Forecast.MonteCarlo(big[^12..], 10, 10_000, seed: 1);

        Assert.That(new[]
        {
            Forecast.Percentile(sim, 0.50), Forecast.Percentile(sim, 0.85),
            Forecast.Percentile(sim, 0.95),
        }, Is.EqualTo(new[] { 9, 12, 13 }));
    }

    [Test]
    public void EightyFivePercentOfPiecesTakeAtMostSixteenDays()
    {
        int[] days = [.. s_items.Select(i => i.Days).Order()];

        Assert.That(days[days.Length / 2], Is.EqualTo(6));
        Assert.That(Forecast.Percentile(days, 0.85), Is.EqualTo(16));
        Assert.That(days[^1], Is.EqualTo(115));
    }
}
