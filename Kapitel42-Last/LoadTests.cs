using NUnit.Framework;

namespace Library.Lending;

// Real clock, one machine: only robust relations are asserted.
public sealed class LoadTests
{
    private LoadResult _four = null!;
    private LoadResult _thirtyTwo = null!;

    [OneTimeSetUp]
    public async Task RunTheLoad()
    {
        using SlowCatalog catalog =
            new(slots: 4, TimeSpan.FromMilliseconds(20), TimeProvider.System);
        _four = await LoadTest.RunAsync(
            catalog.LookupAsync, 4, 512, TimeProvider.System);
        _thirtyTwo = await LoadTest.RunAsync(
            catalog.LookupAsync, 32, 512, TimeProvider.System);
    }

    [Test]
    public void MoreWorkersThanSlotsAddOnlyWaiting()
    {
        Assert.That(_thirtyTwo.PerSecond, Is.LessThan(_four.PerSecond * 1.2));
        Assert.That(_thirtyTwo.Latency.P50,
            Is.GreaterThan(_four.Latency.P50 * 4));
    }

    [Test]
    public void TheClosedLoopObeysLittlesLaw()
    {
        double inFlight = _thirtyTwo.PerSecond * _thirtyTwo.Latency.Mean / 1000;

        Assert.That(inFlight, Is.EqualTo(32).Within(15).Percent);
    }
}
