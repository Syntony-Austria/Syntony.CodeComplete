using NUnit.Framework;

namespace Library.Lending;

public sealed class DoneTests
{
    private static string Done => File.ReadAllText(Paths.Data("done.md"));

    private static ChangeFacts ReturnDeskChange(TestRun run) =>
        ChangeFacts.FromDiff(File.ReadAllText(Paths.Data("return-desk.diff")), run);

    [Test]
    public void AllBoxesTickedIsNotDoneWhenTheFactsDisagree()
    {
        ChangeFacts facts = ReturnDeskChange(new TestRun(41, 0, 3));

        Assert.That(DoneCheck.Problems(Done, facts), Is.EqualTo(new[]
        {
            "tests-changed: ticked, but no test file in the diff",
            "tests-pass: ticked, but 3 skipped",
            "no-todo: ticked, but TODO in an added line",
        }));
    }

    [Test]
    public void AContextLineThatStartsWithPlusIsNotAdded()
    {
        // A leading space marks context, even if "+" follows it.
        string diff = "+++ b/src/Fees.cs\n@@ -1,2 +1,3 @@\n"
            + "     + 25;\n+    int cap = 2000;\n";

        ChangeFacts facts = ChangeFacts.FromDiff(diff, new TestRun(1, 0, 0));

        Assert.That(facts.AddedLines, Is.EqualTo(new[] { "    int cap = 2000;" }));
    }

    [Test]
    public void TheSameBoxesAreDoneWhenTheFactsAgree()
    {
        ChangeFacts facts = new(
            ["src/ReturnDesk.cs", "tests/ReturnDeskTests.cs", "CHANGELOG.md"],
            ["public int FeeOnReturn(DateOnly dueDate) =>"],
            new TestRun(44, 0, 0));

        Assert.That(DoneCheck.Problems(Done, facts), Is.Empty);
    }

    [Test]
    public void AnOpenBoxNeedsAReason()
    {
        ChangeFacts facts = new([], [], new TestRun(1, 0, 0));
        string done = "- [ ] changelog: a line\n" +
            "- [ ] no-todo: none (left out: spike, deleted on Friday)";

        Assert.That(DoneCheck.Problems(done, facts), Is.EqualTo(new[]
        {
            "changelog: not ticked and no reason given",
        }));
    }

    [Test]
    public void ABoxWithoutACheckIsTrusted()
    {
        ChangeFacts facts = new([], [], new TestRun(1, 0, 0));

        Assert.That(DoneCheck.Problems("- [x] reviewed: read", facts), Is.Empty);
    }
}
