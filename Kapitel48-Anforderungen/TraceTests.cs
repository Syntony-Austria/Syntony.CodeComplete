using NUnit.Framework;

namespace Library.Lending;

public sealed class TraceTests
{
    private static string[] Lines =>
        File.ReadAllLines(Paths.Data("requirements.md"));

    // Links from [Requirement] attributes and from the example rows.
    private static List<TestLink> AllLinks(string[] lines) =>
    [
        .. TraceGate.Links(typeof(TraceTests).Assembly),
        .. RequirementsFile.Examples(lines)
            .Select(e => new TestLink(e.Id, "example row")),
    ];

    // The gate: red as soon as one direction breaks.
    [Test]
    public void EveryRequirementHasATestAndEveryTestAKnownId()
    {
        List<Requirement> requirements = RequirementsFile.Requirements(Lines);

        Assert.That(TraceGate.Problems(requirements, AllLinks(Lines)),
            Is.Empty);
    }

    [Test]
    public void TheFileHasTenRequirementsAndNineExamples()
    {
        Assert.That(RequirementsFile.Requirements(Lines), Has.Count.EqualTo(10));
        Assert.That(RequirementsFile.Examples(Lines), Has.Count.EqualTo(9));
    }

    [Test]
    public void ANewRequirementWithoutATestIsReported()
    {
        List<Requirement> requirements =
        [
            .. RequirementsFile.Requirements(Lines),
            new("LEND-011", "A member renews a loan at most twice."),
        ];

        Assert.That(TraceGate.Problems(requirements, AllLinks(Lines)),
            Does.Contain(
                "LEND-011 has no test: A member renews a loan at most twice."));
    }

    [Test]
    public void ATestWithAnUnknownIdIsReported()
    {
        List<TestLink> links =
            [.. AllLinks(Lines), new("LEND-099", "FeeTests.Typo")];

        Assert.That(
            TraceGate.Problems(RequirementsFile.Requirements(Lines), links),
            Does.Contain("FeeTests.Typo names unknown requirement LEND-099"));
    }
}
