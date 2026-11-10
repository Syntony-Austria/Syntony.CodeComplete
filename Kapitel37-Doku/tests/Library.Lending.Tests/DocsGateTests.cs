using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending.Tests;

[TestFixture]
public sealed class DocsGateTests
{
    // DOCS_ROOT lets you point the gate at another folder, e.g. Probe/BrokenDocs.
    private static string Root =>
        Environment.GetEnvironmentVariable("DOCS_ROOT") ?? FindRoot();

    [Test]
    public void EveryAdrHasTheRequiredParts() =>
        Assert.That(AdrGate.Problems(Root), Is.Empty);

    [Test]
    public void EveryAdrNamedInTheCodeExists() =>
        Assert.That(AdrGate.MissingReferences(Root), Is.Empty);

    [Test]
    public void TheProjectVersionHasAChangelogEntry() =>
        Assert.That(ReleaseDocsGate.ChangelogProblems(Root), Is.Empty);

    [Test]
    public void TheReadmeAnswersTheFourQuestions() =>
        Assert.That(ReleaseDocsGate.ReadmeProblems(Root), Is.Empty);

    // Positive control: the gate must find what we broke on purpose.
    [Test]
    public void TheBrokenFixtureFailsEveryGate()
    {
        string broken = Path.Combine(FindRoot(), "Probe", "BrokenDocs");

        Assert.That(AdrGate.Problems(broken), Is.EqualTo(new[]
        {
            "0003-renewal-limit-14-days.md: missing '## Consequences'",
            "0003-renewal-limit-14-days.md: unknown status 'Agreed'",
        }));
        Assert.That(AdrGate.MissingReferences(broken), Is.EqualTo(new[]
        {
            "RenewalPolicy.cs: ADR-0009 does not exist",
        }));
        Assert.That(ReleaseDocsGate.ChangelogProblems(broken), Is.EqualTo(new[]
        {
            "CHANGELOG.md: no entry '## [1.4.0] - <date>'",
            "CHANGELOG.md: unknown kind 'Improvements'",
        }));
        Assert.That(ReleaseDocsGate.ReadmeProblems(broken), Is.EqualTo(new[]
        {
            "README.md: missing '## Status'",
        }));
    }

    [Test]
    public void AReviewDateThatHasPassedIsReported()
    {
        FakeTimeProvider clock = new(new DateTimeOffset(2027, 5, 2, 9, 0, 0,
            TimeSpan.Zero));
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);

        Assert.That(AdrGate.DueForReview(FindRoot(), clock), Is.EqualTo(new[]
        {
            "0002-fees-in-whole-cents.md",
            "0003-renewal-limit-14-days.md",
        }));
    }

    private static string FindRoot()
    {
        DirectoryInfo? dir = new(TestContext.CurrentContext.TestDirectory);
        while (dir is not null
            && !File.Exists(Path.Combine(dir.FullName, "CHANGELOG.md")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException("no CHANGELOG.md above tests");
    }
}
