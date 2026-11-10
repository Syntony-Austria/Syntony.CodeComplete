using NUnit.Framework;

namespace Library.Lending;

// The example rows in requirements.md run as test cases.
public sealed class ExampleTests
{
    private static IEnumerable<TestCaseData> Rows()
    {
        string[] lines = File.ReadAllLines(Paths.Data("requirements.md"));
        foreach (Example e in RequirementsFile.Examples(lines))
        {
            yield return new TestCaseData(e)
                .SetName($"{e.Id} {e.DaysLate} days {e.Kind} ref={e.Reference}")
                .SetProperty(RequirementAttribute.Key, e.Id);
        }
    }

    [TestCaseSource(nameof(Rows))]
    public void TheExampleHolds(Example e)
    {
        Assert.That(LoanFee.InCents(e.DaysLate, e.Kind, e.Reference),
            Is.EqualTo(e.Cents), e.Id);
    }
}
