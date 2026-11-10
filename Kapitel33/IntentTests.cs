using NUnit.Framework;

namespace Library.Lending;

// The numbers come from the library's rules, not from the code.
[TestFixture]
public sealed class IntentTests
{
    [Test]
    public void ThreeDaysLateIsStillFree()
    {
        Assert.That(OverdueFee.For(3, MemberKind.Adult, false), Is.Zero);
    }

    [Test]
    public void TheFourthDayCostsAllFourDays()
    {
        Assert.That(OverdueFee.For(4, MemberKind.Adult, false), Is.EqualTo(100));
    }

    [Test]
    public void AReferenceBookCostsDouble()
    {
        Assert.That(OverdueFee.For(4, MemberKind.Adult, true), Is.EqualTo(200));
    }

    [Test]
    public void AChildPaysHalf()
    {
        Assert.That(OverdueFee.For(4, MemberKind.Child, false), Is.EqualTo(50));
    }

    [Test]
    public void StaffPayNothingEvenAfterAYear()
    {
        Assert.That(OverdueFee.For(365, MemberKind.Staff, false), Is.Zero);
    }

    [Test]
    public void TwentyEurosIsTheMost()
    {
        Assert.That(
            OverdueFee.For(1000, MemberKind.Adult, false), Is.EqualTo(2000));
    }
}
