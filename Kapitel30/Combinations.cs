using NUnit.Framework;

namespace Library.Lending;

// The attribute writes the test cases. The numbers are in the test explorer.
[TestFixture]
public sealed class CombinationTests
{
    [Test, Combinatorial]
    public void AllCombinations(
        [Values(0, 3, 4, 81)] int days,
        [Values] MemberKind member,
        [Values] bool reference)
    {
        Assert.That(OverdueFee.For(days, member, reference), Is.InRange(0, 2000));
    }

    [Test, Pairwise]
    public void EveryPairAtLeastOnce(
        [Values(0, 3, 4, 81)] int days,
        [Values] MemberKind member,
        [Values] bool reference)
    {
        Assert.That(OverdueFee.For(days, member, reference), Is.InRange(0, 2000));
    }
}
