using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run alone: dotnet test -p:Probe=true
[Category("Probe")]
public sealed class ProbeTests
{
    // A ticked list is not proof: this expects "done" and gets three problems.
    [Test]
    public void AllBoxesTickedMeansDone()
    {
        ChangeFacts facts = ChangeFacts.FromDiff(
            File.ReadAllText(Paths.Data("return-desk.diff")),
            new TestRun(41, 0, 3));

        Assert.That(DoneCheck.Problems(
            File.ReadAllText(Paths.Data("done.md")), facts), Is.Empty);
    }

    // Counting hits instead of reviews: one noisy review is not a pattern.
    [Test]
    public void FourHitsMakeACandidate()
    {
        string[] lines = [.. File.ReadAllLines(Paths.Data("review-hints.tsv"))
            .Where(l => l.EndsWith("magic-number"))];

        Assert.That(ReviewHints.Candidates(lines).Select(c => c.Hint),
            Does.Contain("magic-number"));
    }

    // Without a baseline, every old finding blocks as well.
    [Test]
    public void WithoutABaselineOnlyTheNewFindingBlocks()
    {
        List<Finding> found = FeeRuleGate.Find(Paths.Library);

        Assert.That(Baseline.NewFindings(found, []), Has.Count.EqualTo(1));
    }

    // A key with the line number: three blank lines make old findings new.
    [Test]
    public void ALineNumberKeyKeepsOldFindingsOld()
    {
        string copy = FeeRuleTests.CopyOfLibrary();
        string board = Path.Combine(copy, "OverdueBoard.cs");
        File.WriteAllText(board, "\n\n\n" + File.ReadAllText(board));

        string[] baseline = [.. FeeRuleGate.Find(Paths.Library)
            .Where(f => f.File == "OverdueBoard.cs")
            .Select(f => $"{f.Key}|{f.Line}")];
        string[] fresh = [.. FeeRuleGate.Find(copy)
            .Select(f => $"{f.Key}|{f.Line}").Except(baseline)];

        Assert.That(fresh, Has.Length.EqualTo(1));
    }

    // The rule only lives in a text: nothing stops the new desk.
    [Test]
    public void TheNewDeskUsesTheSchedule() =>
        Assert.That(FeeRuleGate.Find(Paths.Library)
            .Where(f => f.File == "ReturnDesk.cs"), Is.Empty);
}
