using System.Globalization;
using System.Text.Json;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class BoundaryTests
{
    [Test]
    public void AnIdLeavesTheProgramAsAnObjectUnlessYouSayOtherwise()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new MemberIdConverter());
        MemberId id = new("m-1");

        Assert.Multiple(() =>
        {
            Assert.That(JsonSerializer.Serialize(id), Is.EqualTo("{\"Value\":\"m-1\"}"));
            Assert.That(JsonSerializer.Serialize(id, options), Is.EqualTo("\"m-1\""));
            Assert.That(JsonSerializer.Deserialize<MemberId>("\"m-1\"", options), Is.EqualTo(id));
        });
    }

    [Test]
    public void AReadOnlyListIsAPromiseNotAWall()
    {
        List<string> titles = ["Faust"];
        IReadOnlyList<string> view = titles;
        ((List<string>)view).Add("Woyzeck");
        IReadOnlyList<string> wrapped = titles.AsReadOnly();

        Assert.Multiple(() =>
        {
            Assert.That(titles, Has.Count.EqualTo(2));
            Assert.That(() => (List<string>)wrapped, Throws.TypeOf<InvalidCastException>());
        });
    }

    [Test]
    public void TextDependsOnTheCultureUnlessYouSayOtherwise()
    {
        CultureInfo turkish = CultureInfo.GetCultureInfo("tr-TR");

        Assert.Multiple(() =>
        {
            Assert.That("TITLE".ToLower(turkish), Is.Not.EqualTo("title"));
            Assert.That("TITLE".ToLowerInvariant(), Is.EqualTo("title"));
            Assert.That(string.Equals("TITLE", "title", StringComparison.OrdinalIgnoreCase), Is.True);
        });
    }

    [Test]
    public void AStructIsCopiedOnAssignment()
    {
        Tally first = new() { Count = 1 };
        Tally second = first;
        second.Count = 2;

        Assert.That(first.Count, Is.EqualTo(1));
    }

    [Test]
    public void EachLoanStateCarriesOnlyItsOwnData()
    {
        DateOnly day = new(2026, 11, 2);

        Assert.Multiple(() =>
        {
            Assert.That(LoanStateText.Describe(new OnShelf()), Is.EqualTo("on the shelf"));
            Assert.That(LoanStateText.Describe(new Lent(new("m-1"), day)), Is.EqualTo("m-1 until 2026-11-02"));
            Assert.That(LoanStateText.Describe(new Lost(day)), Is.EqualTo("lost on 2026-11-02"));
        });
    }

    [Test]
    public void RoundingFollowsTheBankersUnlessYouSayOtherwise()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Math.Round(2.5m), Is.EqualTo(2m));
            Assert.That(Math.Round(3.5m), Is.EqualTo(4m));
            Assert.That(Math.Round(2.5m, MidpointRounding.AwayFromZero), Is.EqualTo(3m));
        });
    }

    [Test]
    public void SplittingCentsLosesWhatDoesNotFit()
    {
        int share = 100 / 3;

        Assert.Multiple(() =>
        {
            Assert.That(share * 3, Is.EqualTo(99));
            Assert.That(100 % 3, Is.EqualTo(1));
        });
    }

    private struct Tally
    {
        public int Count { get; set; }
    }
}
