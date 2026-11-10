using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run only with: dotnet test -p:Probe=true
// Each probe expects what intuition says; the measurement disagrees.
public sealed class ProbeTests
{
    [Test]
    public void OneSlowInAHundredStaysRareForAWideRequest()
    {
        Assert.That(Tail.AnySlow(0.01, 100), Is.LessThan(0.02));
    }

    [Test]
    public void TheAverageOfTwoP99IsTheP99OfBoth()
    {
        double[] quiet = QueueSimulation.Responses(0.5, 200_000, seed: 42);
        double[] busy = QueueSimulation.Responses(0.95, 200_000, seed: 42);
        double averaged =
            (Latency.Summarize(quiet).P99 + Latency.Summarize(busy).P99) / 2;

        Assert.That(Latency.Summarize(quiet.Concat(busy)).P99,
            Is.EqualTo(averaged).Within(5).Percent);
    }

    [Test]
    public void TheWaitGrowsInStepWithTheLoad()
    {
        QueueRun half = QueueSimulation.Run(0.5, 200_000, seed: 42);
        QueueRun full = QueueSimulation.Run(0.95, 200_000, seed: 42);

        Assert.That(full.MeanWait, Is.LessThan(half.MeanWait * 0.95 / 0.5));
    }

    [Test]
    public void TheBucketsTellTheExactP99()
    {
        double[] times = QueueSimulation.Responses(0.95, 200_000, seed: 42);
        Buckets buckets = new(LookupMetrics.Bounds);
        foreach (double ms in times)
        {
            buckets.Add(ms);
        }

        Assert.That(buckets.PercentileAtMost(99),
            Is.EqualTo(Latency.Summarize(times).P99).Within(5).Percent);
    }

    [Test]
    public async Task TheInvariantCheckCatchesALossyDesk()
    {
        IReadOnlyList<Reservation> stored =
            await Stress.RunAsync(new LossyDesk(), senders: 16, each: 500);

        Assert.That(Stress.Broken(stored, 16, 500), Is.Empty);
    }

    // Drops every thousandth reservation, as a bug under load might.
    private sealed class LossyDesk : IDesk
    {
        private readonly ReservationInbox _inner = new();
        private int _seen;

        public ValueTask SendAsync(Reservation reservation) =>
            Interlocked.Increment(ref _seen) % 1000 == 0
                ? ValueTask.CompletedTask
                : _inner.SendAsync(reservation);

        public Task<IReadOnlyList<Reservation>> CloseAsync() =>
            _inner.CloseAsync();
    }
}
