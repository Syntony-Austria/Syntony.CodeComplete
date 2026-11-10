using NUnit.Framework;

namespace Library.Lending;

public sealed class DeskTests
{
    private static readonly DateTimeOffset LendDay =
        new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

    [Test, Requirement("LEND-001")]
    public void ALoanIsDueAfter28Days()
    {
        LoanDesk desk = new(new FixedClock(LendDay));

        Loan loan = desk.Lend("m-1", MemberKind.Adult, false);

        Assert.That(loan.Due, Is.EqualTo(new DateOnly(2026, 11, 30)));
    }

    [Test, Requirement("LEND-006")]
    public void AChildWhoTurnsAdultStillPaysHalf()
    {
        FixedClock clock = new(LendDay);
        LoanDesk desk = new(clock);
        Loan loan = desk.Lend("m-2", MemberKind.Child, false);

        // The card says Adult now; the loan keeps the kind from lending.
        // The amount belongs to LEND-005, so it comes from LoanFee.
        clock.Now = LendDay.AddDays(28 + 5);

        Assert.That(desk.Return(loan, "m-2").Cents,
            Is.EqualTo(LoanFee.InCents(5, MemberKind.Child, false)));
    }

    [Test, Requirement("LEND-009")]
    public void AnAdultReturningAChildsBookChargesTheChildsCard()
    {
        FixedClock clock = new(LendDay);
        LoanDesk desk = new(clock);
        Loan loan = desk.Lend("m-2", MemberKind.Child, false);
        clock.Now = LendDay.AddDays(28 + 4);

        Charge charge = desk.Return(loan, returnedBy: "m-1");

        Assert.That(charge, Is.EqualTo(new Charge("m-2", 50)));
    }

    [Test, Requirement("LEND-007")]
    public void StaffPayNothingEvenAtTheCap()
    {
        Assert.That(LoanFee.InCents(400, MemberKind.Staff, true), Is.Zero);
    }
}
