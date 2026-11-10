using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class TypeTests
{
    [Test]
    public void DoubleDoesNotAddUpLikeMoney()
    {
        double doubles = 0.1 + 0.2;
        decimal decimals = 0.1m + 0.2m;

        Assert.Multiple(() =>
        {
            Assert.That(doubles == 0.3, Is.False);
            Assert.That(decimals == 0.3m, Is.True);
        });
    }

    [Test]
    public void IntegerOverflowIsSilentByDefault()
    {
        int largest = int.MaxValue;

        Assert.Multiple(() =>
        {
            Assert.That(unchecked(largest + 1), Is.EqualTo(int.MinValue));
            Assert.Throws<OverflowException>(() => _ = checked(largest + 1));
        });
    }

    [Test]
    public void FeeRefusesWhatCannotBeAFee()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Fee(-1));
            Assert.Throws<OverflowException>(
                () => _ = new Fee(int.MaxValue) + new Fee(1));
            Assert.That((new Fee(125) + new Fee(100)).ToString(), Is.EqualTo("2.25 EUR"));
        });
    }

    [Test]
    public void AnEnumHoldsAnyNumber()
    {
        MemberRank nonsense = (MemberRank)42;

        Assert.Multiple(() =>
        {
            Assert.That(Enum.IsDefined(nonsense), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => RenewalPolicy.Label(nonsense));
        });
    }

    [Test]
    public void RecordsCompareByValue()
    {
        Assert.That(new MemberId("m-1"), Is.EqualTo(new MemberId("m-1")));
    }

    [Test]
    public void RecordEqualityStopsAtAListInside()
    {
        Shelf left = new("A", ["Faust"]);
        Shelf right = new("A", ["Faust"]);

        Assert.That(left, Is.Not.EqualTo(right));
    }

    [Test]
    public void ChangingARecordLosesItInAHashSet()
    {
        MutableTag tag = new() { Name = "new" };
        HashSet<MutableTag> tags = [tag];

        tag.Name = "old";

        Assert.That(tags.Contains(tag), Is.False);
    }

    [Test]
    public void DefaultSkipsTheConstructorOfAStruct()
    {
        Fee fee = default;
        MemberId member = default;

        Assert.Multiple(() =>
        {
            Assert.That(fee.Cents, Is.Zero);
            Assert.That(member.Value, Is.Null);
        });
    }

    [TestCase("978-3-16-148410-0", ExpectedResult = true)]
    [TestCase("978-3-16-148410-1", ExpectedResult = false)]
    [TestCase("not an isbn", ExpectedResult = false)]
    public bool IsbnAcceptsOnlyValidNumbers(string text) => Isbn.TryParse(text, out _);

    [Test]
    public void ParsedIsbnNeedsNoNullCheck()
    {
        string digits = Isbn.TryParse("978-3-16-148410-0", out Isbn? isbn) ? isbn.Digits : "";

        Assert.That(digits, Is.EqualTo("9783161484100"));
    }

    private sealed record Shelf(string Name, List<string> Titles);

    private sealed record MutableTag
    {
        public required string Name { get; set; }
    }
}
