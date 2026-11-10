using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class DecisionTableTests
{
    private static readonly object[][] s_rules =
    [
        // days, member, reference, cents, why
        [2, MemberKind.Adult, false, 0, "within the grace days"],
        [4, MemberKind.Adult, false, 100, "all four days count"],
        [4, MemberKind.Adult, true, 200, "a reference book costs double"],
        [4, MemberKind.Child, false, 50, "a child pays half"],
        [4, MemberKind.Child, true, 100, "both rules together"],
        [400, MemberKind.Staff, true, 0, "staff never pay"],
        [400, MemberKind.Child, true, 1000, "half of the maximum"],
    ];

    [TestCaseSource(nameof(s_rules))]
    public void TheLibrarysTable(
        int days, MemberKind member, bool reference, int cents, string why)
    {
        Assert.That(
            OverdueFee.For(days, member, reference), Is.EqualTo(cents), why);
    }
}
