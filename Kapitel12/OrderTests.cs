using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class OrderTests
{
    private static readonly IReadOnlyList<Loan> s_loans = [new("1", "Faust", 10, false)];

    [Test]
    public void TheFieldHidesTheOrderAndAnswersZero()
    {
        FeeRunTangled run = new();

        int forgotten = run.Compute();
        run.Load(s_loans);

        Assert.Multiple(() =>
        {
            Assert.That(forgotten, Is.Zero);
            Assert.That(run.Compute(), Is.EqualTo(250));
        });
    }

    [Test]
    public void TheParameterShowsTheOrder()
    {
        Assert.That(FeeRun.TotalCents(FeeRun.FindDue(s_loans)), Is.EqualTo(250));
    }

    [Test]
    public void StaffPayNothing()
    {
        IReadOnlyList<Loan> staffLoans = [new("2", "Faust", 10, true)];
        FeeRunTangled run = new();
        run.Load(staffLoans);

        Assert.Multiple(() =>
        {
            Assert.That(run.Compute(), Is.Zero);
            Assert.That(FeeRun.TotalCents(FeeRun.FindDue(staffLoans)), Is.Zero);
        });
    }
}
