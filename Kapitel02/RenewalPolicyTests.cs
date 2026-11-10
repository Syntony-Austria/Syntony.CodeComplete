using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class RenewalPolicyTests
{
    private static readonly DateOnly s_today = new(2026, 11, 1);

    [Test]
    public void BothVersionsAgreeOnEveryCombination()
    {
        foreach (bool hasReservation in new[] { false, true })
        {
            foreach (bool isPrioritised in new[] { false, true })
            {
                for (int renewals = 0; renewals <= 3; renewals++)
                {
                    for (int daysOverdue = -5; daysOverdue <= 20; daysOverdue++)
                    {
                        DateOnly due = s_today.AddDays(-daysOverdue);
                        RenewalRequest request = new(
                            renewals, due, s_today, hasReservation, isPrioritised);

                        Assert.That(
                            RenewalPolicy.MayRenew(request),
                            Is.EqualTo(RenewalCheckTangled.Chk(
                                renewals, due, s_today,
                                hasReservation, isPrioritised)));
                    }
                }
            }
        }
    }

    [TestCase(1, 0, false, false, ExpectedResult = true)]
    [TestCase(2, 0, false, false, ExpectedResult = false)] // no third renewal
    [TestCase(0, 14, false, false, ExpectedResult = true)] // last day of the window
    [TestCase(0, 15, false, false, ExpectedResult = false)] // window closed
    [TestCase(0, 0, true, false, ExpectedResult = false)] // someone is waiting
    [TestCase(0, 0, true, true, ExpectedResult = true)] // prioritised go first
    public bool MayRenewFollowsTheLibraryRules(
        int renewals, int daysOverdue, bool hasReservation, bool isPrioritised)
    {
        RenewalRequest request = new(
            renewals,
            s_today.AddDays(-daysOverdue),
            s_today,
            hasReservation,
            isPrioritised);

        return RenewalPolicy.MayRenew(request);
    }
}
