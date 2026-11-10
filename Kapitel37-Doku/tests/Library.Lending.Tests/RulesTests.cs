using NUnit.Framework;

namespace Library.Lending.Tests;

[TestFixture]
public sealed class RulesTests
{
    private static readonly DateOnly s_due = new(2026, 11, 1);

    [TestCase(14, true, Description = "last day of ADR-0003")]
    [TestCase(15, false, Description = "renewal window closed")]
    public void RenewalEndsFourteenDaysAfterTheDueDate(int daysLate, bool expected)
    {
        RenewalRequest request =
            new(0, s_due, s_due.AddDays(daysLate), false, false);

        Assert.That(RenewalPolicy.MayRenew(request), Is.EqualTo(expected));
    }

    [TestCase(3, 0)]
    [TestCase(4, 25)]
    [TestCase(200, 2000)]
    public void FeeIsWholeCentsWithGraceAndCap(int daysLate, int expectedCents) =>
        Assert.That(
            new FeeSchedule().FeeInCents(daysLate), Is.EqualTo(expectedCents));
}
