using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

public sealed class RetryTests
{
    private static readonly RetryOptions Options = new(
        4, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(2));

    internal static async Task<T> DriveAsync<T>(
        Task<T> task, FakeTimeProvider clock)
    {
        for (int step = 0; step < 10_000 && !task.IsCompleted; step++)
        {
            clock.Advance(TimeSpan.FromMilliseconds(10));
        }

        return await task;
    }

    [Test]
    public async Task TheNaiveRetryOrdersTheBookTwice()
    {
        PartnerLibraryFake partner = new(answersToLose: 1);
        NaiveLending naive = new(new SilentCatalog(), partner);

        string order = await naive.OrderAsync("978", "m-1");

        Assert.That(order, Is.EqualTo("order-2"));
        Assert.That(partner.Orders, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task AKeyTurnsTheSecondTryIntoTheSameOrder()
    {
        PartnerLibraryFake partner = new(answersToLose: 1);
        const string Key = "m-1/978/2026-11-08";

        Assert.That(
            () => partner.OrderAsync("978", "m-1", Key, CancellationToken.None),
            Throws.TypeOf<TimeoutException>());
        string order = await partner.OrderAsync(
            "978", "m-1", Key, CancellationToken.None);

        Assert.That(order, Is.EqualTo("order-1"));
        Assert.That(partner.Orders, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ATransientFailureIsRetriedWithGrowingCeilings()
    {
        FakeTimeProvider clock = new();
        ScriptedRemote remote = new("catalog.busy", "catalog.busy");
        Retry retry = new(clock, new Random(39), Options);

        Result<string> result = await DriveAsync(
            retry.RunAsync(remote.CallAsync, CancellationToken.None), clock);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(remote.Calls, Is.EqualTo(3));
        Assert.That(retry.Waits[0], Is.LessThan(TimeSpan.FromMilliseconds(100)));
        Assert.That(retry.Waits[1], Is.LessThan(TimeSpan.FromMilliseconds(200)));
    }

    [Test]
    public async Task WithoutTheClockMovingTheRetryWaits()
    {
        FakeTimeProvider clock = new();
        ScriptedRemote remote = new("catalog.busy");
        Retry retry = new(clock, new Random(39), Options);

        Task<Result<string>> running =
            retry.RunAsync(remote.CallAsync, CancellationToken.None);
        int callsBefore = remote.Calls;
        bool doneBefore = running.IsCompleted;
        await DriveAsync(running, clock);

        Assert.That((callsBefore, doneBefore), Is.EqualTo((1, false)));
        Assert.That(remote.Calls, Is.EqualTo(2));
    }

    [TestCase("catalog.not_found", 1)]
    [TestCase("catalog.strange", 1)]
    [TestCase("catalog.busy", 2)]
    public async Task OnlyATransientFailureIsTriedAgain(string code, int calls)
    {
        FakeTimeProvider clock = new();
        ScriptedRemote remote = new(code);
        Retry retry = new(clock, new Random(39), Options);

        await DriveAsync(
            retry.RunAsync(remote.CallAsync, CancellationToken.None), clock);

        Assert.That(remote.Calls, Is.EqualTo(calls));
    }

    [Test]
    public async Task TheRetryStopsAtMaxAttempts()
    {
        FakeTimeProvider clock = new();
        ScriptedRemote remote = new([.. Enumerable.Repeat("catalog.busy", 10)]);
        Retry retry = new(clock, new Random(39), Options);

        Result<string> result = await DriveAsync(
            retry.RunAsync(remote.CallAsync, CancellationToken.None), clock);

        Assert.That(result.Error?.Code, Is.EqualTo("catalog.busy"));
        Assert.That(remote.Calls, Is.EqualTo(4));
    }

    [Test]
    public void NoJitterGivesEveryClientTheSameWait()
    {
        TimeSpan first = TimeSpan.FromMilliseconds(100);
        TimeSpan cap = TimeSpan.FromSeconds(10);

        TimeSpan[] waits = [.. Enumerable.Range(0, 8)
            .Select(retry => Backoff.Exponential(retry, first, cap))];

        Assert.That(waits.Select(w => (int)w.TotalMilliseconds),
            Is.EqualTo(new[] { 100, 200, 400, 800, 1600, 3200, 6400, 10000 }));
    }
}
