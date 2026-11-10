using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class OverdueFeeEquivalenceTests
{
    private static readonly DateOnly s_today = new(2026, 11, 1);

    [Test]
    public void BothVersionsAgreeOnEveryCombination()
    {
        foreach (MemberKind member in Enum.GetValues<MemberKind>())
        {
            foreach (bool isReference in new[] { false, true })
            {
                for (int daysLate = -5; daysLate <= 60; daysLate++)
                {
                    DateOnly due = s_today.AddDays(-daysLate);
                    decimal tangled = OverdueFeeTangled.Calc(
                        due, s_today, (int)member, isReference, 8m, waived: false);
                    decimal clean = OverdueFee.For(
                        new Loan(due, isReference, 8m), member, s_today);

                    Assert.That(
                        clean,
                        Is.EqualTo(tangled),
                        $"{member}, reference={isReference}, late={daysLate}");
                }
            }
        }
    }
}
