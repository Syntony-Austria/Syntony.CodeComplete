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
}
