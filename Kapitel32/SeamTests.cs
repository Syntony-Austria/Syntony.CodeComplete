using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NUnit.Framework;

namespace Library.Lending;

public sealed class SealedService
{
    public string Name() => "real";
}

public class OpenService
{
    public string Name() => "real";

    public virtual string Title() => "real";
}

[TestFixture]
public sealed class SeamTests
{
    private static readonly DateTimeOffset s_noon =
        new(2026, 12, 31, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void TheClockIsSetByTheTest()
    {
        FakeTimeProvider clock = new(s_noon);
        DueDates dates = new(clock);

        Assert.That(dates.ForNewLoan(), Is.EqualTo(new DateOnly(2027, 1, 28)));
    }

    [Test]
    public void OverdueStartsTheDayAfterTheDueDate()
    {
        FakeTimeProvider clock = new(s_noon);
        DueDates dates = new(clock);
        DateOnly due = new(2026, 12, 31);

        Assert.That(dates.IsOverdue(due), Is.False);
        clock.Advance(TimeSpan.FromHours(12));
        Assert.That(dates.IsOverdue(due), Is.True);
    }

    [Test]
    public async Task ADayPassesInNoTime()
    {
        FakeTimeProvider clock = new(s_noon);
        Task<string> waiting = new ReminderWait(clock).AfterOneDayAsync("m-1");

        Assert.That(waiting.IsCompleted, Is.False);
        clock.Advance(TimeSpan.FromDays(1));

        Assert.That(await waiting, Is.EqualTo("Reminder for m-1"));
    }

    [Test]
    public void AddingADayIsNotTheSameAs24HoursOverTheClockChange()
    {
        TimeZoneInfo vienna = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
        DateTimeOffset saturdayNoon = new(2026, 3, 28, 11, 0, 0, TimeSpan.Zero);
        DateTimeOffset later = saturdayNoon.AddHours(24);

        DateTimeOffset local = TimeZoneInfo.ConvertTime(later, vienna);

        Assert.That(local.Hour, Is.EqualTo(13));
    }

    [Test]
    public void TheSameSeedPicksTheSameWinner()
    {
        string[] members = ["a", "b", "c", "d", "e"];

        string first = new Raffle(new Random(42)).Winner(members);
        string second = new Raffle(new Random(42)).Winner(members);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void ARealFileInATemporaryFolderIsCheapEnough()
    {
        string folder = Directory.CreateTempSubdirectory("library-").FullName;
        try
        {
            LoanFile file = new(Path.Combine(folder, "loans.txt"));
            file.Add("m-1;Faust");

            Assert.That(file.All(), Is.EqualTo(new[] { "m-1;Faust" }));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void SubstituteRefusesASealedClass()
    {
        Assert.Throws<TypeLoadException>(
            () => Substitute.For<SealedService>());
    }

    [Test]
    public void SubstituteLeavesANonVirtualMethodAlone()
    {
        OpenService service = Substitute.For<OpenService>();

        Assert.Multiple(() =>
        {
            Assert.That(service.Name(), Is.EqualTo("real"));
            Assert.That(service.Title(), Is.Empty);
        });
    }
}
