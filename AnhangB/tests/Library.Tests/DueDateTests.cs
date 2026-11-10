using System.Globalization;
using NUnit.Framework;

namespace Library.Tests;

[TestFixture]
public sealed class DueDateTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    [TestCase("2026-11-02", false)]
    [TestCase("2026-11-01", true)]
    public void OverdueOnlyAfterTheDueDay(string due, bool expected)
    {
        DateTimeOffset now = new(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);
        DueDate sut = new(new FixedClock(now));
        DateOnly day = DateOnly.Parse(due, CultureInfo.InvariantCulture);
        Assert.That(sut.IsOverdue(day), Is.EqualTo(expected));
    }
}
