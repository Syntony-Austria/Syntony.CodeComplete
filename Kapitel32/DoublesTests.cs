using NSubstitute;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class DoublesTests
{
    [Test]
    public void AStubAndASpyShowWhoGetsMail()
    {
        SpyMailer mailer = new();

        int sent = new Reminders(new StubLoanStore("a", "b"), mailer).SendAll();

        Assert.Multiple(() =>
        {
            Assert.That(sent, Is.EqualTo(2));
            Assert.That(mailer.Sent, Is.EqualTo(new[] { "a", "b" }));
        });
    }

    [Test]
    public void TheFakeShowsTheResultAndSurvivesTheRefactoring()
    {
        FakeLoanStore store = new("a", "b");

        new RemindersRefactored(store, new SpyMailer()).SendAll();

        Assert.That(store.Reminded, Is.EquivalentTo(new[] { "a", "b" }));
    }

    [Test]
    public void TheMockKnowsTheOldCode()
    {
        ILoanStore store = Substitute.For<ILoanStore>();
        store.OverdueMembers().Returns(["a", "b"]);

        new Reminders(store, Substitute.For<IMailer>()).SendAll();

        store.Received(1).OverdueMembers();
    }

    [Test]
    public void TheSameMockFailsAfterTheRefactoring()
    {
        ILoanStore store = Substitute.For<ILoanStore>();
        store.OverdueMembers().Returns(["a", "b"]);

        new RemindersRefactored(store, Substitute.For<IMailer>()).SendAll();

        Assert.Throws<NSubstitute.Exceptions.ReceivedCallsException>(
            () => store.Received(1).OverdueMembers());
    }

    [Test]
    public void AMockThatForgetsTheAnswerReturnsEmptyAndTheTestStaysGreen()
    {
        ILoanStore store = Substitute.For<ILoanStore>();

        int sent = new Reminders(store, Substitute.For<IMailer>()).SendAll();

        Assert.That(sent, Is.Zero);
    }
}
