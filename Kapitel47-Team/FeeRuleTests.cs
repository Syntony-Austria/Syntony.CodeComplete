using NUnit.Framework;

namespace Library.Lending;

public sealed class FeeRuleTests
{
    [Test]
    public void TheInlineFeeForgetsTheCap()
    {
        DateTimeOffset now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);
        ReturnDesk desk = new(new FixedClock(now));
        DateOnly due = new DateOnly(2026, 11, 8).AddDays(-100);

        Assert.That(desk.FeeOnReturn(due), Is.EqualTo(2425));
        Assert.That(FeeSchedule.FeeInCents(100), Is.EqualTo(2000));
    }

    [Test]
    public void TheGateFindsEveryFeeOutsideTheSchedule()
    {
        List<Finding> found = FeeRuleGate.Find(Paths.Library);

        Assert.That(found.Select(f => $"{f.File}:{f.Line}"), Is.EqualTo(new[]
        {
            "OverdueBoard.cs:7", "OverdueBoard.cs:10", "ReturnDesk.cs:10",
        }));
    }

    [Test]
    public void OnlyTheNewFindingBlocks()
    {
        List<Finding> found = FeeRuleGate.Find(Paths.Library);
        List<string> baseline = Baseline.Read(Paths.Data("fee-baseline.txt"));

        Assert.That(Baseline.NewFindings(found, baseline).Select(f => f.File),
            Is.EqualTo(new[] { "ReturnDesk.cs" }));
    }

    [Test]
    public void AnOldFindingStaysOldWhenLinesMove()
    {
        string copy = CopyOfLibrary();
        string board = Path.Combine(copy, "OverdueBoard.cs");
        File.WriteAllText(board, "\n\n\n" + File.ReadAllText(board));

        List<Finding> found = FeeRuleGate.Find(copy);
        List<string> baseline = Baseline.Read(Paths.Data("fee-baseline.txt"));

        Assert.That(found.Count(f => f.File == "OverdueBoard.cs" && f.Line > 10),
            Is.EqualTo(1));
        Assert.That(Baseline.NewFindings(found, baseline).Select(f => f.File),
            Is.EqualTo(new[] { "ReturnDesk.cs" }));
    }

    [Test]
    public void AFixedFindingMustLeaveTheBaseline()
    {
        string copy = CopyOfLibrary();
        File.Delete(Path.Combine(copy, "OverdueBoard.cs"));

        List<Finding> found = FeeRuleGate.Find(copy);
        List<string> baseline = Baseline.Read(Paths.Data("fee-baseline.txt"));

        Assert.That(Baseline.Stale(found, baseline), Has.Count.EqualTo(2));
    }

    [Test]
    public void TheBaselineHasNoStaleEntryToday() =>
        Assert.That(Baseline.Stale(FeeRuleGate.Find(Paths.Library),
            Baseline.Read(Paths.Data("fee-baseline.txt"))), Is.Empty);

    internal static string CopyOfLibrary()
    {
        string copy = Path.Combine(Path.GetTempPath(), "k47-" + Guid.NewGuid());
        Directory.CreateDirectory(copy);
        foreach (string file in Directory.GetFiles(Paths.Library))
        {
            File.Copy(file, Path.Combine(copy, Path.GetFileName(file)));
        }

        return copy;
    }
}

internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
