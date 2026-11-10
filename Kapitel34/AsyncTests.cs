using System.Diagnostics;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class AsyncTests
{
    private static readonly DateTimeOffset s_start =
        new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task AThousandWaitsNeedNoThreads()
    {
        Stopwatch watch = Stopwatch.StartNew();

        await Task.WhenAll(
            Enumerable.Range(0, 1000).Select(_ => Waiting.FreeAsync(200)));

        TestContext.Out.WriteLine($"free 1000 x 200 ms: {watch.ElapsedMilliseconds} ms");
        Assert.That(watch.ElapsedMilliseconds, Is.LessThan(2000));
    }

    [Test]
    public async Task BlockedThreadsStandInLine()
    {
        ThreadPool.GetMinThreads(out int workers, out _);
        Stopwatch watch = Stopwatch.StartNew();

        await Task.WhenAll(
            Enumerable.Range(0, 200).Select(_ => Waiting.BlockingAsync(200)));

        TestContext.Out.WriteLine(
            $"blocking 200 x 200 ms: {watch.ElapsedMilliseconds} ms, min {workers}");
        Assert.That(watch.ElapsedMilliseconds, Is.GreaterThan(300));
    }

    [Test]
    public async Task AnAsyncMethodRunsOnTheCallersThreadUntilItReallyWaits()
    {
        int caller = Environment.CurrentManagedThreadId;
        int before = -1;

        async Task WorkAsync()
        {
            before = Environment.CurrentManagedThreadId;
            await Task.Delay(1);
        }

        Task running = WorkAsync();

        Assert.That(before, Is.EqualTo(caller));
        await running;
    }

    [Test]
    public void BlockingOnTheOnlyThreadNeverEnds()
    {
        SingleThreadContext context = new();
        Task<bool> run = Task.Factory.StartNew(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            return LoanService.CountAsync().Wait(TimeSpan.FromMilliseconds(500));
        }, TaskCreationOptions.LongRunning);

        Assert.That(run.Result, Is.False);
    }

    [Test]
    public void ConfigureAwaitFalseDoesNotNeedThatThread()
    {
        SingleThreadContext context = new();
        Task<bool> run = Task.Factory.StartNew(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            return LoanService.CountFreeAsync().Wait(TimeSpan.FromMilliseconds(500));
        }, TaskCreationOptions.LongRunning);

        Assert.That(run.Result, Is.True);
    }

    [Test]
    public void AnAsyncVoidExceptionPassesTheCatch()
    {
        SingleThreadContext context = new();
        bool caught = false;
        Thread thread = new(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                LoanService.SaveAndForget();
            }
            catch (InvalidOperationException)
            {
                caught = true;
            }

            context.Pump(TimeSpan.FromMilliseconds(100));
        });
        thread.Start();
        thread.Join();

        Assert.Multiple(() =>
        {
            Assert.That(caught, Is.False);
            Assert.That(context.Crashes, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void ATaskCarriesItsExceptionToWhoeverWaits()
    {
        Assert.ThrowsAsync<InvalidOperationException>(LoanService.SaveAsync);
    }

    [Test]
    public async Task WhenAllThrowsOnlyTheFirstButKeepsAll()
    {
        Task all = Task.WhenAll(
            Task.FromException(new InvalidOperationException("one")),
            Task.FromException(new TimeoutException("two")));
        Exception? first = null;

        try
        {
            await all;
        }
        catch (InvalidOperationException ex)
        {
            first = ex;
        }

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.Null);
            Assert.That(all.Exception!.InnerExceptions, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task ASelectStartsTheWorkEachTimeItIsRead()
    {
        int started = 0;
        IEnumerable<Task> tasks = Enumerable.Range(0, 3).Select(async _ =>
        {
            Interlocked.Increment(ref started);
            await Task.Yield();
        });

        await Task.WhenAll(tasks);
        await Task.WhenAll(tasks);

        Assert.That(started, Is.EqualTo(6));
    }

    [Test]
    public async Task ATokenThatIsIgnoredStopsNothing()
    {
        ReminderRun run = new(TimeProvider.System);
        using CancellationTokenSource source = new();
        source.Cancel();

        await run.IgnoresTokenAsync(100, source.Token);

        Assert.That(run.Sent, Is.EqualTo(100));
    }

    [Test]
    public async Task AHonouredTokenStopsWithinOneStep()
    {
        FakeTimeProvider clock = new(s_start);
        ReminderRun run = new(clock);
        using CancellationTokenSource source = new();
        Task task = run.SendAllAsync(100, source.Token);

        clock.Advance(TimeSpan.FromSeconds(1));
        clock.Advance(TimeSpan.FromSeconds(1));
        await Task.Delay(50);
        int sentBefore = run.Sent;
        source.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(() => task);
        Assert.Multiple(() =>
        {
            Assert.That(sentBefore, Is.EqualTo(2));
            Assert.That(run.Sent, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task ATimeoutIsATokenWithAClock()
    {
        FakeTimeProvider clock = new(s_start);
        using CancellationTokenSource source =
            new(TimeSpan.FromSeconds(30), clock);
        Task waiting = Task.Delay(Timeout.Infinite, source.Token);

        clock.Advance(TimeSpan.FromSeconds(29));
        Assert.That(waiting.IsCompleted, Is.False);
        clock.Advance(TimeSpan.FromSeconds(1));

        await Task.WhenAny(waiting);
        Assert.That(waiting.IsCanceled, Is.True);
    }

    [TestCase(100, 4)]
    [TestCase(100, 1)]
    public async Task ASemaphoreLimitsHowManyRun(int count, int limit)
    {
        int highest = await Limits.RunAsync(count, limit);

        Assert.That(highest, Is.EqualTo(limit));
    }

    [Test]
    public async Task ParallelForEachAsyncAlsoHasALimit()
    {
        int running = 0;
        int highest = 0;
        ParallelOptions options = new() { MaxDegreeOfParallelism = 3 };

        await Parallel.ForEachAsync(Enumerable.Range(0, 50), options, async (_, token) =>
        {
            int now = Interlocked.Increment(ref running);
            lock (options)
            {
                highest = Math.Max(highest, now);
            }

            await Task.Delay(2, token);
            Interlocked.Decrement(ref running);
        });

        Assert.That(highest, Is.LessThanOrEqualTo(3));
    }
}
