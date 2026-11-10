using NUnit.Framework;

namespace Library.Lending;

public sealed class StressTests
{
    [Test]
    [Repeat(5)]
    public async Task SixteenSendersLoseNothingAndDoubleNothing()
    {
        IReadOnlyList<Reservation> stored =
            await Stress.RunAsync(new ReservationInbox(), senders: 16, each: 500);

        Assert.That(Stress.Broken(stored, 16, 500), Is.Empty);
    }
}
