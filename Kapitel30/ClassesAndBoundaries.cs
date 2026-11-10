using NUnit.Framework;

namespace Library.Lending;

// One value from each class: free, charged, capped.
[TestFixture]
public sealed class ClassTests
{
    [TestCase(1, 0)]
    [TestCase(10, 250)]
    [TestCase(500, 2000)]
    public void OneValuePerClass(int days, int cents)
    {
        Assert.That(
            OverdueFee.For(days, MemberKind.Adult, false), Is.EqualTo(cents));
    }
}

// The same classes, tested where they touch.
[TestFixture]
public sealed class BoundaryTests
{
    [TestCase(int.MinValue, 0)]
    [TestCase(0, 0)]
    [TestCase(3, 0)]
    [TestCase(4, 100)]
    [TestCase(79, 1975)]
    [TestCase(80, 2000)]
    [TestCase(81, 2000)]
    public void EachSideOfEachBoundary(int days, int cents)
    {
        Assert.That(
            OverdueFee.For(days, MemberKind.Adult, false), Is.EqualTo(cents));
    }

    [Test]
    public void AHugeNumberOfDaysDoesNotOverflow()
    {
        int fee = OverdueFee.For(int.MaxValue, MemberKind.Adult, false);

        Assert.That(fee, Is.EqualTo(2000));
    }
}
