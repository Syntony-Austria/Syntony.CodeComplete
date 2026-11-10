using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class SecondErrorTests
{
    [Test]
    public void TheEdgeTurnsEachTrackIntoItsOwnAnswer()
    {
        List<string> log = [];
        ReservationEndpoint endpoint = new(new ReservationDesk(1), log);

        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Post("m-1"), Is.EqualTo(new Reply(201, "position 1")));
            Assert.That(endpoint.Post("m-2"), Is.EqualTo(new Reply(409, "reservation.queue_full")));
            Assert.That(endpoint.Post(" "), Is.EqualTo(new Reply(400, "member_id.invalid")));
            Assert.That(log, Is.Empty);
        });
    }

    [Test]
    public void AFailureHalfwayLeavesTheWrongState()
    {
        List<string> waiting = ["m-1"];
        List<string> lent = [];

        Assert.Throws<IOException>(() => new LoanHandoverTangled(waiting, lent)
            .Hand("m-1", _ => throw new IOException("disk")));

        Assert.Multiple(() =>
        {
            Assert.That(waiting, Is.Empty);
            Assert.That(lent, Is.Empty);
        });
    }

    [Test]
    public void TheRightOrderLeavesTheOldState()
    {
        List<string> waiting = ["m-1"];
        List<string> lent = [];

        Assert.Throws<IOException>(() => new LoanHandover(waiting, lent)
            .Hand("m-1", _ => throw new IOException("disk")));

        Assert.That(waiting, Is.EqualTo(new[] { "m-1" }));
    }

    [Test]
    public async Task ACancelledTaskIsNoError()
    {
        using CancellationTokenSource source = new();
        await source.CancelAsync();

        Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(1000, source.Token));
        Assert.That(new TaskCanceledException(), Is.InstanceOf<OperationCanceledException>());
    }
}
