using NUnit.Framework;

namespace Library.Lending;

public sealed class ChangeTests
{
    private static string[] Old => File.ReadAllLines(Paths.Data("requirements.md"));

    private static string[] New =>
        File.ReadAllLines(Paths.Data("requirements-v2.md"));

    public static IEnumerable<TestCaseData> NewRows() =>
        RequirementsFile.Examples(New).Select(e => new TestCaseData(e)
            .SetName($"v2 {e.Id} {e.DaysLate} days {e.Kind}"));

    private static List<string> Changed() => Impact.Changed(
        RequirementsFile.Requirements(Old), RequirementsFile.Requirements(New));

    [Test]
    public void TwoSentencesChanged()
    {
        Assert.That(Changed(), Is.EqualTo(new[] { "LEND-005", "LEND-009" }));
    }

    [Test]
    public void TheChangeNamesFourTestsToReread()
    {
        List<TestLink> links =
        [
            .. TraceGate.Links(typeof(ChangeTests).Assembly),
            .. RequirementsFile.Examples(Old)
                .Select(e => new TestLink(e.Id, $"example {e.DaysLate} days")),
        ];

        Assert.That(Impact.ToReread(Changed(), links).Select(l => l.Test),
            Is.EquivalentTo(new[]
            {
                "FeeTests.HalfOfAnOddFeeRoundsDown",
                "DeskTests.AnAdultReturningAChildsBookChargesTheChildsCard",
                "example 4 days", "example 5 days",
            }));
    }

    // Against the new file, today's code fails one example of nine.
    [Test]
    public void OnlyOneNewExampleFailsTodaysCode()
    {
        List<Example> failing = [.. RequirementsFile.Examples(New).Where(e =>
            LoanFee.InCents(e.DaysLate, e.Kind, e.Reference) != e.Cents)];

        Assert.That(failing.Select(e => $"{e.Id} {e.DaysLate}"),
            Is.EqualTo(new[] { "LEND-005 5" }));
    }
}
