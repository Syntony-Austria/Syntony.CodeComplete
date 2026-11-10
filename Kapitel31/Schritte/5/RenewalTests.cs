using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class RenewalTests
{
    [Test]
    public void AFreshLoanMayBeRenewed()
    {
        Assert.That(Renewals.MayRenew(renewedSoFar: 0, isReserved: false), Is.True);
    }

    [Test]
    public void AfterTwoRenewalsTheLoanMustComeBack()
    {
        Assert.That(Renewals.MayRenew(renewedSoFar: 2, isReserved: false), Is.False);
    }

    [Test]
    public void AReservedBookMustComeBack()
    {
        Assert.That(Renewals.MayRenew(renewedSoFar: 0, isReserved: true), Is.False);
    }
}
