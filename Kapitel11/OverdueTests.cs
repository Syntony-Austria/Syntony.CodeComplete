using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class OverdueTests
{
    [Test]
    public void AskingTwiceChangesNothing()
    {
        OverdueCheck check = new();

        Assert.Multiple(() =>
        {
            Assert.That(OverdueCheck.IsOverdue(10), Is.True);
            Assert.That(OverdueCheck.IsOverdue(10), Is.True);
            Assert.That(check.ReminderCount, Is.Zero);
        });
    }
}
