using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class LoanLineTests
{
    private const string Title = "Faust; Der Tragoedie erster Teil";

    [Test]
    public void TheReportedTitleSurvivesTheFirstFix()
    {
        string line = LoanLine.Format("m-1", Title);

        Assert.That(LoanLine.ParseFirstFix(line).Title, Is.EqualTo(Title));
    }

    [Test]
    public void TheFirstFixStillBreaksOnTheNeighbour()
    {
        string line = LoanLine.Format("m;1", "Faust");

        Assert.That(LoanLine.ParseFirstFix(line).Member, Is.EqualTo("m"));
    }

    [TestCase("m-1", "Faust; Erster Teil")]
    [TestCase("m;1", "Faust")]
    [TestCase("100%", "50%3B")]
    [TestCase("", "")]
    public void TheSafeFormatKeepsEverything(string member, string title)
    {
        string line = LoanLine.FormatSafe(member, title);

        Assert.That(LoanLine.ParseSafe(line), Is.EqualTo((member, title)));
    }
}
