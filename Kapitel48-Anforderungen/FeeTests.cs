using NUnit.Framework;

namespace Library.Lending;

public sealed class FeeTests
{
    [Test, Requirement("LEND-002"), Requirement("LEND-003")]
    public void TheFourthDayChargesAllFourDays()
    {
        Assert.That(LoanFee.InCents(3, MemberKind.Adult, false), Is.Zero);
        Assert.That(LoanFee.InCents(4, MemberKind.Adult, false), Is.EqualTo(100));
    }

    [Test, Requirement("LEND-005")]
    public void HalfOfAnOddFeeRoundsDown()
    {
        Assert.That(LoanFee.InCents(5, MemberKind.Child, false), Is.EqualTo(62));
    }

    [Test, Requirement("LEND-008")]
    public void TheCapComesBeforeTheChildsHalf()
    {
        Assert.That(LoanFee.InCents(400, MemberKind.Child, true),
            Is.EqualTo(LoanFee.CapInCents / 2));
    }
}
