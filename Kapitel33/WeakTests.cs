using NUnit.Framework;

namespace Library.Lending;

// Every line runs. Almost nothing is checked.
[TestFixture]
public sealed class WeakTests
{
    [TestCase(1, MemberKind.Adult, false)]
    [TestCase(9, MemberKind.Adult, false)]
    [TestCase(9, MemberKind.Adult, true)]
    [TestCase(9, MemberKind.Child, false)]
    [TestCase(9, MemberKind.Staff, false)]
    public void TheFeeCanBeCalculated(int days, MemberKind member, bool reference)
    {
        int fee = OverdueFee.For(days, member, reference);

        Assert.That(fee, Is.GreaterThanOrEqualTo(0));
    }
}
