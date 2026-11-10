using NUnit.Framework;

namespace Library.Lending;

public sealed record MemberId(string Value);

[TestFixture]
public sealed class MemberIdContractTests : EqualityContract<MemberId>
{
    protected override MemberId Value() => new("m-1");

    protected override MemberId SameValue() => new("m-1");

    protected override MemberId OtherValue() => new("m-2");
}

// Deliberately wrong: Equals without a matching GetHashCode.
public sealed class TangledId(string value)
{
    public string Value { get; } = value;

    public override bool Equals(object? obj) =>
        obj is TangledId other && other.Value == Value;

#pragma warning disable CA1065 // The book shows what this rule is for.
    public override int GetHashCode() => base.GetHashCode();
#pragma warning restore CA1065
}

[TestFixture]
public sealed class TheContractCatchesTheMistake
{
    [Test]
    public void TwoEqualTangledIdsAreLostInADictionary()
    {
        Dictionary<TangledId, int> loans = new() { [new TangledId("m-1")] = 3 };

        Assert.Multiple(() =>
        {
            Assert.That(new TangledId("m-1").Equals(new TangledId("m-1")), Is.True);
            Assert.That(loans.ContainsKey(new TangledId("m-1")), Is.False);
        });
    }
}
