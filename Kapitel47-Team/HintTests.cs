using NUnit.Framework;

namespace Library.Lending;

public sealed class HintTests
{
    private static string[] Lines =>
        File.ReadAllLines(Paths.Data("review-hints.tsv"));

    [Test]
    public void AHintFromThreeReviewsBecomesACandidate()
    {
        Assert.That(ReviewHints.Candidates(Lines), Is.EqualTo(new[]
        {
            new Candidate("fee-outside-schedule", 3, "2026-09-02", "2026-10-20"),
        }));
    }

    [Test]
    public void FourHitsInOneReviewAreOneReview()
    {
        string[] lines = [.. Lines.Where(l => l.EndsWith("magic-number"))];

        Assert.That(lines, Has.Length.EqualTo(4));
        Assert.That(ReviewHints.Candidates(lines), Is.Empty);
    }

    [Test]
    public void ThreeDifferentReviewsAreEnough()
    {
        string[] lines =
        [
            "2026-11-02\tPR-250\tnaming",
            .. Lines.Where(l => l.EndsWith("naming")),
        ];

        Assert.That(ReviewHints.Candidates(lines).Single().Hint,
            Is.EqualTo("naming"));
    }
}
