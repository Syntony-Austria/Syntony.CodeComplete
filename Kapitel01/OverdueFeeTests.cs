using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class OverdueFeeTests
{
    private static readonly DateOnly s_today = new(2026, 11, 1);

    [TestCase(3, 0.00)] // last day of grace costs nothing
    [TestCase(4, 1.00)] // from day four, every late day counts
    [TestCase(40, 8.00)] // never more than the price of the book
    public void AdultPaysPerLateDayAfterGraceUpToBookPrice(int daysLate, double expected)
    {
        Loan loan = new(s_today.AddDays(-daysLate), IsReference: false, BookPrice: 8m);

        Assert.That(OverdueFee.For(loan, MemberKind.Adult, s_today), Is.EqualTo((decimal)expected));
    }

    [Test]
    public void EveryLoanStatusHasItsOwnLabel()
    {
        LoanStatus[] all = Enum.GetValues<LoanStatus>();

        Assert.That(all.Select(LoanStatusText.Label).Distinct().Count(), Is.EqualTo(all.Length));
    }

    [Test]
    public void StaffNeverPays()
    {
        Loan loan = new(s_today.AddDays(-30), IsReference: true, BookPrice: 50m);

        Assert.That(OverdueFee.For(loan, MemberKind.Staff, s_today), Is.Zero);
    }

    [Test]
    public void ChildOnReferenceBookPaysTheAdultRate()
    {
        Loan loan = new(s_today.AddDays(-5), IsReference: true, BookPrice: 50m);

        Assert.That(OverdueFee.For(loan, MemberKind.Child, s_today), Is.EqualTo(1.25m));
    }

    [Test]
    public void ChapterQuestion_CapIsAppliedAfterTheChildDiscount()
    {
        Loan loan = new(s_today.AddDays(-5), IsReference: true, BookPrice: 1m);

        Assert.That(OverdueFee.For(loan, MemberKind.Child, s_today), Is.EqualTo(1.00m));
    }

    [Test]
    public void Revised_CapIsAppliedBeforeTheChildDiscount()
    {
        Loan loan = new(s_today.AddDays(-5), IsReference: true, BookPrice: 1m);

        Assert.That(OverdueFeeRevised.For(loan, MemberKind.Child, s_today), Is.EqualTo(0.50m));
    }

    [Test]
    public void Revised_ChangesNothingForAdults()
    {
        Loan loan = new(s_today.AddDays(-5), IsReference: true, BookPrice: 1m);

        Assert.That(OverdueFeeRevised.For(loan, MemberKind.Adult, s_today), Is.EqualTo(OverdueFee.For(loan, MemberKind.Adult, s_today)));
    }
}
