using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class PathAndPatternTests
{
    private static readonly string s_root = Path.Combine(Path.GetTempPath(), "library");

    [Test]
    public void DotsLeaveTheFolder()
    {
        Assert.That(SafePath.Resolve(s_root, "../secret.txt"), Is.Null);
    }

    [Test]
    public void AnAbsoluteNameReplacesTheFolder()
    {
        string other = Path.Combine(Path.GetTempPath(), "other.txt");

        Assert.Multiple(() =>
        {
            Assert.That(Path.Combine(s_root, other), Is.EqualTo(other));
            Assert.That(SafePath.Resolve(s_root, other), Is.Null);
        });
    }

    [Test]
    public void TheFolderNextDoorFoolsThePrefix()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SafePath.IsInsideTangled(s_root, "../library-secret/x.txt"), Is.True);
            Assert.That(SafePath.Resolve(s_root, "../library-secret/x.txt"), Is.Null);
        });
    }

    [Test]
    public void ANormalNameStaysInside()
    {
        Assert.That(SafePath.Resolve(s_root, "a/b.txt"), Is.EqualTo(Path.Combine(s_root, "a", "b.txt")));
    }

    [Test]
    public void ATimeLimitStopsTheBadPattern()
    {
        string input = new string('a', 40) + "!";

        Assert.Throws<RegexMatchTimeoutException>(() => Patterns.WithLimit().IsMatch(input));
    }

    [Test]
    public void TheLinearEngineAnswersAtOnce()
    {
        string input = new string('a', 40) + "!";

        Assert.That(Patterns.Linear().IsMatch(input), Is.False);
    }
}
