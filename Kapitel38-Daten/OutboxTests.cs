using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class OutboxTests
{
    private const string Pending =
        "SELECT COUNT(*) FROM Outbox WHERE SentAt IS NULL";

    private static void BrokerDown(Notice notice) =>
        throw new IOException("broker not reachable");

    [Test]
    public void PublishingAfterTheCommitCanLoseTheNotice()
    {
        using TempDb db = new();

        Assert.Throws<IOException>(() => OutboxLending.LendThenPublish(
            db.Connect(), "m-1", "b-7", BrokerDown));

        // The loan exists; nobody will ever hear about it.
        Assert.That(db.Count("SELECT COUNT(*) FROM Loan"), Is.EqualTo(1));
    }

    [Test]
    public void TheOutboxKeepsTheNoticeUntilItIsSent()
    {
        using TempDb db = new();
        SqliteConnection desk = db.Connect();
        OutboxLending.LendWithNotice(desk, "m-1", "b-7");
        OutboxSender sender = new(desk, new FakeTimeProvider());
        NoticeDesk receiver = new();

        Assert.Throws<IOException>(() => sender.SendPending(BrokerDown));
        long stillPending = db.Count(Pending);
        int sent = sender.SendPending(receiver.Receive);

        Assert.Multiple(() =>
        {
            Assert.That(stillPending, Is.EqualTo(1));
            Assert.That(sent, Is.EqualTo(1));
            Assert.That(db.Count(Pending), Is.EqualTo(0));
            Assert.That(receiver.Handled, Is.EqualTo(1));
        });
    }

    [Test]
    public void ACrashBeforeMarkingSendsTwiceAndTheDeskHandlesOnce()
    {
        using TempDb db = new();
        SqliteConnection desk = db.Connect();
        OutboxLending.LendWithNotice(desk, "m-1", "b-7");
        OutboxSender sender = new(desk, new FakeTimeProvider());
        NoticeDesk receiver = new();
        bool crashed = false;

        // Delivered, then the sender dies before it can mark the row.
        void DeliverThenCrash(Notice notice)
        {
            receiver.Receive(notice);
            if (!crashed)
            {
                crashed = true;
                throw new IOException("sender stopped");
            }
        }

        Assert.Throws<IOException>(() => sender.SendPending(DeliverThenCrash));
        sender.SendPending(DeliverThenCrash);

        Assert.Multiple(() =>
        {
            Assert.That(receiver.Delivered, Is.EqualTo(2));
            Assert.That(receiver.Handled, Is.EqualTo(1));
        });
    }

    [Test]
    public void TheSenderStampsTheTimeFromItsClock()
    {
        using TempDb db = new();
        SqliteConnection desk = db.Connect();
        OutboxLending.LendWithNotice(desk, "m-1", "b-7");
        FakeTimeProvider clock = new(
            new DateTimeOffset(2026, 11, 8, 9, 30, 0, TimeSpan.Zero));

        new OutboxSender(desk, clock).SendPending(_ => { });

        using SqliteCommand read = new("SELECT SentAt FROM Outbox", desk);
        Assert.That(read.ExecuteScalar(),
            Is.EqualTo("2026-11-08T09:30:00.0000000+00:00"));
    }
}
