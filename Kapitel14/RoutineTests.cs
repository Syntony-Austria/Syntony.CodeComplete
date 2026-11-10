using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class RoutineTests
{
    private static readonly DateOnly s_due = new(2026, 11, 1);

    [Test]
    public void BothDesksAgreeOnEveryCase()
    {
        int compared = 0;
        foreach (MemberKind member in Enum.GetValues<MemberKind>())
        {
            foreach (bool isReference in new[] { false, true })
            {
                foreach (decimal price in new[] { 1m, 5m, 50m })
                {
                    for (int days = -5; days <= 400; days++)
                    {
                        AssertSame(member, isReference, price, days);
                        compared++;
                    }
                }
            }
        }

        Assert.That(compared, Is.EqualTo(7308));
    }

    [Test]
    public void TwoAmountsSwapWithoutAWord()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SwappedArguments.Remaining(10m, 4m), Is.EqualTo(6m));
            Assert.That(SwappedArguments.Remaining(4m, 10m), Is.EqualTo(-6m));
            Assert.That(SwappedArguments.Remaining(paid: 4m, fee: 10m), Is.EqualTo(6m));
        });
    }

    [Test]
    public void TheFeeCanBeTestedWithoutADeskOrALog()
    {
        BookReturn book = new("m-1", "Faust", s_due, MemberKind.Adult, false, 50m);

        Assert.That(ReturnDesk.FeeFor(book, s_due.AddDays(10)), Is.EqualTo(2.5m));
    }

    private static void AssertSame(
        MemberKind member, bool isReference, decimal price, int days)
    {
        DateOnly today = s_due.AddDays(days);
        List<string> oldLog = [];
        List<string> newLog = [];
        string oldReceipt = new ReturnDeskTangled().ReturnBook(
            "m-1", "Faust", s_due, today, member == MemberKind.Staff,
            member == MemberKind.Child, isReference, price, oldLog);
        string newReceipt = new ReturnDesk(newLog).Return(
            new BookReturn("m-1", "Faust", s_due, member, isReference, price), today);

        Assert.That(newReceipt, Is.EqualTo(oldReceipt));
        Assert.That(newLog, Is.EqualTo(oldLog));
    }
}
