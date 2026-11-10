using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class LoanStateTests
{
    [TestCase(LoanState.Open, LoanEvent.Renew, LoanState.Renewed)]
    [TestCase(LoanState.Open, LoanEvent.Return, LoanState.Returned)]
    [TestCase(LoanState.Renewed, LoanEvent.Return, LoanState.Returned)]
    public void TheAllowedSteps(LoanState from, LoanEvent happens, LoanState to)
    {
        Assert.That(LoanStates.Next(from, happens), Is.EqualTo(to));
    }

    // All pairs, also those nobody thought of: a new enum value is included.
    [Test, Combinatorial]
    public void EveryOtherStepIsRefused(
        [Values] LoanState from, [Values] LoanEvent happens)
    {
        bool allowed = (from, happens) is
            (LoanState.Open, _) or (LoanState.Renewed, LoanEvent.Return);

        Assert.That(LoanStates.Next(from, happens) is not null, Is.EqualTo(allowed));
    }

    [Test]
    public void AfterTheReturnNothingHappensAnymore()
    {
        Assert.Multiple(() =>
        {
            Assert.That(LoanStates.Next(LoanState.Returned, LoanEvent.Renew), Is.Null);
            Assert.That(LoanStates.Next(LoanState.Returned, LoanEvent.Return), Is.Null);
        });
    }
}
