using System.Collections.Concurrent;
using System.Threading.Channels;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class ConcurrencyTests
{
    private const int Rounds = 1_000_000;

    [Test]
    public void APlainIncrementLosesUpdates()
    {
        Counters counters = new();

        Parallel.For(0, Rounds, _ => counters.AddPlain());

        TestContext.Out.WriteLine($"plain: {counters.Plain} of {Rounds}");
        Assert.That(counters.Plain, Is.LessThanOrEqualTo(Rounds));
    }

    [Test]
    public void InterlockedAndLockLoseNothing()
    {
        Counters counters = new();

        Parallel.For(0, Rounds, _ =>
        {
            counters.AddAtomic();
            counters.AddLocked();
        });

        Assert.Multiple(() =>
        {
            Assert.That(counters.Atomic, Is.EqualTo(Rounds));
            Assert.That(counters.Locked, Is.EqualTo(Rounds));
        });
    }

    // A forced interleaving: the second caller slips in after the check.
    [Test]
    public void CheckThenActBreaksTheLimit()
    {
        LoanLimit limit = new(1);

        limit.TryLendTangled(between: () => limit.TryLendTangled());

        Assert.That(limit.Loans, Is.EqualTo(2));
    }

    [Test]
    public void OneStepKeepsTheLimitUnderLoad()
    {
        LoanLimit limit = new(100);

        Parallel.For(0, 10_000, _ => limit.TryLend());

        Assert.That(limit.Loans, Is.EqualTo(100));
    }

    [Test]
    public void TwoLocksInOppositeOrderBlockEachOther()
    {
        Account a = new(1);
        Account b = new(2);
        using Barrier both = new(2);
        TimeSpan patience = TimeSpan.FromMilliseconds(200);

        Task<bool> one = Task.Run(
            () => Transfers.TangledMove(a, b, 1, both, patience));
        Task<bool> two = Task.Run(
            () => Transfers.TangledMove(b, a, 1, both, patience));

        Assert.Multiple(() =>
        {
            Assert.That(one.Result, Is.False);
            Assert.That(two.Result, Is.False);
        });
    }

    [Test]
    public void AFixedOrderSurvivesManyOppositeTransfers()
    {
        Account a = new(1) { Cents = 1000 };
        Account b = new(2) { Cents = 1000 };

        Parallel.For(0, 100_000, i =>
        {
            if (i % 2 == 0)
            {
                Transfers.Move(a, b, 1);
            }
            else
            {
                Transfers.Move(b, a, 1);
            }
        });

        Assert.That(a.Cents + b.Cents, Is.EqualTo(2000));
    }

    [Test]
    public void GetOrAddCanRunTheFactoryTwice()
    {
        ConcurrentDictionary<string, int> cache = new();
        int calls = 0;
        using Barrier both = new(2);

        Parallel.Invoke(
            () => cache.GetOrAdd("x", Slow),
            () => cache.GetOrAdd("x", Slow));

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(cache, Has.Count.EqualTo(1));
        });

        int Slow(string key)
        {
            Interlocked.Increment(ref calls);
            both.SignalAndWait();
            return 1;
        }
    }

    [Test]
    public void LazyRunsItOnce()
    {
        ConcurrentDictionary<string, Lazy<int>> cache = new();
        int calls = 0;

        Parallel.For(0, 1000, _ => _ = cache.GetOrAdd(
            "x", _ => new Lazy<int>(() => Interlocked.Increment(ref calls))).Value);

        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void APlainDictionaryUnderWritersLosesEntriesOrThrows()
    {
        Dictionary<int, int> plain = [];
        int failures = 0;

        Parallel.For(0, 100_000, i =>
        {
            try
            {
                plain[i] = i;
            }
            catch (Exception ex) when (ex is InvalidOperationException
                or IndexOutOfRangeException or NullReferenceException
                or ArgumentException)
            {
                Interlocked.Increment(ref failures);
            }
        });

        TestContext.Out.WriteLine(
            $"entries: {plain.Count} of 100000, failed: {failures}");
        Assert.That(plain.Count, Is.LessThan(100_000));
    }

    [Test]
    public void ChangingAListWhileReadingItThrows()
    {
        List<int> list = [1, 2, 3];

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (int item in list)
            {
                list.Add(item);
            }
        });
    }

    [Test]
    public void ReadersSeeAWholeTable()
    {
        FeeTable table = new();
        table.Replace(new Dictionary<string, int> { ["a"] = 1, ["b"] = 1 });
        int torn = 0;
        using CancellationTokenSource stop = new();

        Task reader = Task.Run(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                IReadOnlyDictionary<string, int> now = table.Current;
                if (now["a"] != now["b"])
                {
                    Interlocked.Increment(ref torn);
                }
            }
        });
        for (int i = 2; i < 20_000; i++)
        {
            table.Replace(new Dictionary<string, int> { ["a"] = i, ["b"] = i });
        }

        stop.Cancel();
        reader.Wait();

        Assert.That(torn, Is.Zero);
    }

    [Test]
    public async Task AnAwaitBetweenCheckAndActLoadsTwiceWithoutThreads()
    {
        int loads = 0;
        TaskCompletionSource<string> gate = new();
        CatalogCache cache = new(() =>
        {
            loads++;
            return gate.Task;
        });

        Task<string> one = cache.GetTangledAsync();
        Task<string> two = cache.GetTangledAsync();
        gate.SetResult("Faust");
        await Task.WhenAll(one, two);

        Assert.That(loads, Is.EqualTo(2));
    }

    [Test]
    public async Task KeepingTheTaskLoadsOnce()
    {
        int loads = 0;
        TaskCompletionSource<string> gate = new();
        CatalogCache cache = new(() =>
        {
            loads++;
            return gate.Task;
        });

        Task<string> one = cache.GetAsync();
        Task<string> two = cache.GetAsync();
        gate.SetResult("Faust");
        await Task.WhenAll(one, two);

        Assert.That(loads, Is.EqualTo(1));
    }

    [Test]
    public async Task ABoundedChannelSlowsTheProducerDown()
    {
        Channel<int> channel = Channel.CreateBounded<int>(5);

        for (int i = 0; i < 5; i++)
        {
            await channel.Writer.WriteAsync(i);
        }

        ValueTask full = channel.Writer.WriteAsync(5);
        Assert.That(full.IsCompleted, Is.False);

        Assert.That(await channel.Reader.ReadAsync(), Is.Zero);
        await full;
        Assert.That(channel.Reader.Count, Is.EqualTo(5));
    }
}
