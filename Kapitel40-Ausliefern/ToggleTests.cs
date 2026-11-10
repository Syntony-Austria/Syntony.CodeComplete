using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

public sealed class ToggleTests
{
    private static readonly IConfiguration Empty =
        new ConfigurationBuilder().Build();

    public static FakeTimeProvider On(int year, int month, int day) =>
        new(new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero));

    [Test]
    public void TheInventoryIsCleanOnItsLastDay()
    {
        IReadOnlyList<string> problems = ToggleCheck.Problems(
            ToggleInventory.All, ToggleInventory.Retired, Empty,
            On(2027, 3, 31));

        Assert.That(problems, Is.Empty);
    }

    [Test]
    public void AReleaseToggleReportsItselfTheDayAfter()
    {
        IReadOnlyList<string> problems = ToggleCheck.Problems(
            ToggleInventory.All, ToggleInventory.Retired, Empty,
            On(2027, 4, 1));

        Assert.That(problems, Is.EqualTo(new[]
        {
            "Reminders.Scale2026: overdue since 2027-03-31",
        }));
    }

    [TestCase("Reminders.Fast", "Reminders.Fast: retired name")]
    [TestCase("Reminders.Scale206", "Reminders.Scale206: not in the inventory")]
    public void ASettingOutsideTheInventoryIsRefused(string key, string problem)
    {
        IConfiguration settings = ReminderTests.With($"Toggles:{key}", "true");

        IReadOnlyList<string> problems = ToggleCheck.Problems(
            ToggleInventory.All, ToggleInventory.Retired, settings,
            On(2026, 11, 8));

        Assert.That(problems, Is.EqualTo(new[] { problem }));
    }

    [Test]
    public void AReleaseToggleWithoutDateIsRefused()
    {
        Toggle undated = new("Fees.NewCap", ToggleKind.Release, "lending", null);

        IReadOnlyList<string> problems = ToggleCheck.Problems(
            [undated], ToggleInventory.Retired, Empty, On(2026, 11, 8));

        Assert.That(problems, Is.EqualTo(new[]
        {
            "Fees.NewCap: release toggle without date",
        }));
    }

    [Test]
    public void AnOldNameInTheInventoryIsRefused()
    {
        Toggle reused = new("Reminders.Fast", ToggleKind.Release, "lending",
            new DateOnly(2027, 3, 31));

        IReadOnlyList<string> problems = ToggleCheck.Problems(
            [reused], ToggleInventory.Retired, Empty, On(2026, 11, 8));

        Assert.That(problems, Is.EqualTo(new[]
        {
            "Reminders.Fast: retired name in the inventory",
        }));
    }

    [Test]
    public void AnUnknownNameInTheCodeIsAProgrammingError()
    {
        Toggles toggles = new(Empty);

        Assert.That(toggles.IsOn("Reminders.Pause"), Is.False);
        Assert.Throws<ArgumentException>(() => toggles.IsOn("Reminders.Fast"));
    }

    // Uses the real clock on purpose: run it in the health run, not the build.
    [Test, Category("Health"), Explicit("health run only")]
    public void NothingInTheInventoryIsOverdueToday()
    {
        IReadOnlyList<string> problems = ToggleCheck.Problems(
            ToggleInventory.All, ToggleInventory.Retired, Empty,
            TimeProvider.System);

        Assert.That(problems, Is.Empty);
    }
}
