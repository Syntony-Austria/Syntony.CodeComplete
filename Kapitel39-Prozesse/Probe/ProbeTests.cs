using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run only with: dotnet test -p:Probe=true
// Each probe expects the good behaviour from the bad variant.
public sealed class ProbeTests
{
    [Test]
    public void TheNaiveLookupEndsWithinTwoSeconds()
    {
        FakeTimeProvider clock = new();
        NaiveLending naive = new(new SilentCatalog(), new PartnerLibraryFake(0));

        Task<string?> title = naive.TitleAsync("978");
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.That(title.IsCompleted, Is.True);
    }

    [Test]
    public async Task TheNaiveRetryOrdersOnce()
    {
        PartnerLibraryFake partner = new(answersToLose: 1);
        NaiveLending naive = new(new SilentCatalog(), partner);

        await naive.OrderAsync("978", "m-1");

        Assert.That(partner.Orders, Has.Count.EqualTo(1));
    }

    [Test]
    public void CancelAfterOnALinkedSourceFollowsTheFakeClock()
    {
        FakeTimeProvider clock = new();
        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);

        linked.CancelAfter(TimeSpan.FromSeconds(2));
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.That(linked.IsCancellationRequested, Is.True);
    }

    [Test]
    public async Task BreakerOutsideStopsAfterThreeCalls()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 3, TimeSpan.FromSeconds(30));
        Retry retry = new(clock, new Random(39), new(
            5, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(2)));
        ScriptedRemote remote = new([.. Enumerable.Repeat("catalog.busy", 20)]);

        for (int i = 0; i < 3; i++)
        {
            await RetryTests.DriveAsync(
                breaker.RunAsync(
                    ct => retry.RunAsync(remote.CallAsync, ct),
                    CancellationToken.None),
                clock);
        }

        Assert.That(remote.Calls, Is.EqualTo(3));
    }

    [Test]
    public void TheStormWithoutJitterStaysBelowHalfTheClients()
    {
        StormResult none = RetryStorm.Run(1000, Spread.None, seed: 39);

        Assert.That(none.Peak, Is.LessThan(500));
    }
}
