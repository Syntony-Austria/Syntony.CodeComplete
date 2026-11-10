using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

public sealed class BreakerTests
{
    private static readonly TimeSpan Break = TimeSpan.FromSeconds(30);

    private static string[] Busy(int count) =>
        [.. Enumerable.Repeat("catalog.busy", count)];

    [Test]
    public async Task ThreeFailuresOpenTheCircuitAndNothingIsSent()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 3, Break);
        ScriptedRemote remote = new(Busy(10));

        for (int i = 0; i < 3; i++)
        {
            await breaker.RunAsync(remote.CallAsync, CancellationToken.None);
        }

        Result<string> fourth =
            await breaker.RunAsync(remote.CallAsync, CancellationToken.None);

        Assert.That(breaker.State, Is.EqualTo(CircuitState.Open));
        Assert.That(fourth.Error?.Code, Is.EqualTo("circuit.open"));
        Assert.That(remote.Calls, Is.EqualTo(3));
    }

    [Test]
    public async Task AfterTheBreakOneGoodProbeClosesTheCircuit()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 3, Break);
        ScriptedRemote remote = new(Busy(3));
        List<CircuitState> seen = [];

        for (int i = 0; i < 3; i++)
        {
            await breaker.RunAsync(remote.CallAsync, CancellationToken.None);
        }

        seen.Add(breaker.State);
        clock.Advance(Break - TimeSpan.FromSeconds(1));
        await breaker.RunAsync(remote.CallAsync, CancellationToken.None);
        seen.Add(breaker.State);
        clock.Advance(TimeSpan.FromSeconds(1));
        Result<string> probe =
            await breaker.RunAsync(remote.CallAsync, CancellationToken.None);
        seen.Add(breaker.State);

        Assert.That(seen, Is.EqualTo(new[]
            { CircuitState.Open, CircuitState.Open, CircuitState.Closed }));
        Assert.That(probe.IsSuccess, Is.True);
        Assert.That(remote.Calls, Is.EqualTo(4));
    }

    [Test]
    public async Task ABadProbeOpensTheCircuitAgain()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 3, Break);
        ScriptedRemote remote = new(Busy(4));

        for (int i = 0; i < 3; i++)
        {
            await breaker.RunAsync(remote.CallAsync, CancellationToken.None);
        }

        clock.Advance(Break);
        await breaker.RunAsync(remote.CallAsync, CancellationToken.None);

        Assert.That(breaker.State, Is.EqualTo(CircuitState.Open));
        Assert.That(remote.Calls, Is.EqualTo(4));
    }

    [Test]
    public async Task TheHalfOpenCircuitLetsOnlyOneProbeThrough()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 1, Break);
        TaskCompletionSource<Result<string>> slow = new();
        int calls = 0;

        await breaker.RunAsync(
            _ => Task.FromResult(Result<string>.Failure(new("catalog.busy", "x"))),
            CancellationToken.None);
        clock.Advance(Break);
        Task<Result<string>> probe = breaker.RunAsync(
            _ => { calls++; return slow.Task; }, CancellationToken.None);
        Result<string> second = await breaker.RunAsync(
            _ => { calls++; return slow.Task; }, CancellationToken.None);
        slow.SetResult(Result<string>.Success("ok"));
        await probe;

        Assert.That(second.Error?.Code, Is.EqualTo("circuit.open"));
        Assert.That(calls, Is.EqualTo(1));
        Assert.That(breaker.State, Is.EqualTo(CircuitState.Closed));
    }

    [Test]
    public async Task AProbeThatThrowsFreesTheSlotForTheNextCaller()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 1, Break);
        await breaker.RunAsync(
            _ => Task.FromResult(Result<string>.Failure(new("catalog.busy", "x"))),
            CancellationToken.None);
        clock.Advance(Break);

        Assert.ThrowsAsync<OperationCanceledException>(
            () => breaker.RunAsync<string>(
                _ => throw new OperationCanceledException(),
                CancellationToken.None));
        Result<string> next = await breaker.RunAsync(
            _ => Task.FromResult(Result<string>.Success("ok")),
            CancellationToken.None);

        Assert.That(next.IsSuccess, Is.True);
        Assert.That(breaker.State, Is.EqualTo(CircuitState.Closed));
    }

    [Test]
    public async Task NotFoundIsAnAnswerAndKeepsTheCircuitClosed()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 3, Break);
        ScriptedRemote remote = new(
            [.. Enumerable.Repeat("catalog.not_found", 5)]);

        for (int i = 0; i < 5; i++)
        {
            await breaker.RunAsync(remote.CallAsync, CancellationToken.None);
        }

        Assert.That(breaker.State, Is.EqualTo(CircuitState.Closed));
        Assert.That(remote.Calls, Is.EqualTo(5));
    }

    [Test]
    public async Task RetryOutsideBreakerInsideStopsAfterThreeCalls()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 3, Break);
        Retry retry = new(clock, new Random(39), new(
            5, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(2)));
        ScriptedRemote remote = new(Busy(20));

        Result<string> result = await RetryTests.DriveAsync(
            retry.RunAsync(
                ct => breaker.RunAsync(remote.CallAsync, ct),
                CancellationToken.None),
            clock);

        Assert.That(result.Error?.Code, Is.EqualTo("circuit.open"));
        Assert.That(remote.Calls, Is.EqualTo(3));
    }

    [Test]
    public async Task BreakerOutsideRetryInsideNeedsFifteenCalls()
    {
        FakeTimeProvider clock = new();
        CircuitBreaker breaker = new(clock, 3, Break);
        Retry retry = new(clock, new Random(39), new(
            5, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(2)));
        ScriptedRemote remote = new(Busy(20));

        for (int i = 0; i < 3; i++)
        {
            await RetryTests.DriveAsync(
                breaker.RunAsync(
                    ct => retry.RunAsync(remote.CallAsync, ct),
                    CancellationToken.None),
                clock);
        }

        Assert.That(breaker.State, Is.EqualTo(CircuitState.Open));
        Assert.That(remote.Calls, Is.EqualTo(15));
    }
}
