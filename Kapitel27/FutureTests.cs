using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class FutureTests
{
    private static string Golden(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Golden", name));

    [TestCase("loan-v1.json", "Hauptstelle")]
    [TestCase("loan-v2.json", "Hauptstelle")]
    [TestCase("loan-v3.json", "Linz")]
    public void EveryFileEverWrittenStillLoads(string file, string branch)
    {
        LoanRecord? loan = LoanRecordFile.Read(Golden(file));

        Assert.That(loan, Is.EqualTo(new LoanRecord("m-1", new DateOnly(2026, 12, 1), branch)));
    }

    [Test]
    public void TheNewestFormatSurvivesARoundTrip()
    {
        LoanRecord loan = new("m-1", new DateOnly(2026, 12, 1), "Linz");

        Assert.That(LoanRecordFile.Read(LoanRecordFile.Write(loan)), Is.EqualTo(loan));
    }

    [Test]
    public void AFileFromTheFutureIsRefusedNotGuessed()
    {
        Assert.That(LoanRecordFile.Read("""{"version":99}"""), Is.Null);
    }

    [Test]
    public void DuringTheChangeOldAndNewRowsReadTheSame()
    {
        MemberRow old = new() { FullName = "Maria Muster" };
        MemberRow fresh = new();
        MemberNameMigration.Write(fresh, "Maria", "Muster");

        Assert.Multiple(() =>
        {
            Assert.That(MemberNameMigration.Display(old), Is.EqualTo("Maria Muster"));
            Assert.That(MemberNameMigration.Display(fresh), Is.EqualTo("Maria Muster"));
            Assert.That(fresh.FullName, Is.EqualTo("Maria Muster"));
        });
    }

    [Test]
    public void TheBackfillCanRunTwice()
    {
        MemberRow row = new() { FullName = "Anna Maria Muster" };

        MemberNameMigration.Backfill(row);
        MemberNameMigration.Backfill(row);

        Assert.Multiple(() =>
        {
            Assert.That(row.FirstName, Is.EqualTo("Anna Maria"));
            Assert.That(row.LastName, Is.EqualTo("Muster"));
        });
    }

    [Test]
    public void TheShadowReportsDifferencesAndKeepsTheOldAnswer()
    {
        List<string> reports = [];
        ShadowFee fee = new(days => days * 25, days => days > 30 ? days * 50 : days * 25, reports.Add);

        Assert.Multiple(() =>
        {
            Assert.That(fee.For(10), Is.EqualTo(250));
            Assert.That(reports, Is.Empty);
            Assert.That(fee.For(40), Is.EqualTo(1000));
            Assert.That(reports, Is.EqualTo(new[] { "days 40: old 1000, new 2000" }));
        });
    }

    [Test]
    public void ABrokenNewRuleDoesNotHurt()
    {
        List<string> reports = [];
        ShadowFee fee = new(days => days, _ => throw new DivideByZeroException(), reports.Add);

        Assert.Multiple(() =>
        {
            Assert.That(fee.For(3), Is.EqualTo(3));
            Assert.That(reports.Single(), Does.Contain("DivideByZeroException"));
        });
    }

    [Test]
    public void ASunsetReportsItselfWhenItIsOver()
    {
        Sunset sunset = new("x", new DateOnly(2026, 12, 31));

        Assert.Multiple(() =>
        {
            Assert.That(sunset.IsOver(new DateOnly(2026, 12, 31)), Is.False);
            Assert.That(sunset.IsOver(new DateOnly(2027, 1, 1)), Is.True);
            Assert.That(Sunsets.All.Where(s => s.IsOver(new DateOnly(2026, 11, 2))), Is.Empty);
        });
    }
}
