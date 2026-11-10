using NUnit.Framework;

namespace Library.Lending;

// The expected value is the code once more. Whatever changes, changes in both.
[TestFixture]
public sealed class MirrorTests
{
    [Test]
    public void TheFeeIsDaysTimesRate()
    {
        int fee = OverdueFee.For(9, MemberKind.Adult, false);

        Assert.That(fee, Is.EqualTo(9 * OverdueFee.CentsPerDay));
    }

    [Test]
    public void WithinTheGraceDaysThereIsNoFee()
    {
        int fee = OverdueFee.For(OverdueFee.GraceDays, MemberKind.Adult, false);

        Assert.That(fee, Is.Zero);
    }

    [Test]
    public void TheFeeStopsAtTheMaximum()
    {
        int fee = OverdueFee.For(1000, MemberKind.Adult, false);

        Assert.That(fee, Is.EqualTo(OverdueFee.MaxCents));
    }
}
