using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class DaysLateTests
{
    private static readonly DateTimeOffset s_due =
        new(2026, 12, 1, 23, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset s_nextMorning =
        new(2026, 12, 2, 8, 0, 0, TimeSpan.Zero);

    // Report: returned the next morning, and the system says "not late".
    [Test]
    public void TheOldCodeShowsTheBug()
    {
        Assert.That(DaysLate.Tangled(s_due, s_nextMorning), Is.Zero);
    }

    [Test]
    public void TheNextMorningIsOneDayLate()
    {
        int days = DaysLate.Between(
            DateOnly.FromDateTime(s_due.Date),
            DateOnly.FromDateTime(s_nextMorning.Date));

        Assert.That(days, Is.EqualTo(1));
    }

    // Neighbours of the bug: where else can a calendar day go wrong?
    [TestCase("2026-12-31", "2027-01-01", 1)]
    [TestCase("2028-02-28", "2028-02-29", 1)]
    [TestCase("2026-03-28", "2026-03-30", 2)]
    [TestCase("2026-12-02", "2026-12-01", 0)]
    public void TheNeighbours(string due, string returned, int days)
    {
        int actual = DaysLate.Between(
            DateOnly.Parse(due), DateOnly.Parse(returned));

        Assert.That(actual, Is.EqualTo(days));
    }
}
