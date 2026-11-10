using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class DeadlockTests
{
    // Emergency exit: a test that expects a deadlock must not hang itself.
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);

    [Test]
    public void OppositeOrderIsReportedOnceAndBothFinish()
    {
        WatchedLock loans = new("loans");
        WatchedLock reminders = new("reminders");
        using Barrier both = new(2);
        int reported = 0;

        void Run(WatchedLock first, WatchedLock second)
        {
            try
            {
                using WatchedLock.Scope a = first.Enter();
                both.SignalAndWait();
                using WatchedLock.Scope b = second.Enter();
            }
            catch (DeadlockException)
            {
                Interlocked.Increment(ref reported);
            }
        }

        Task one = Task.Run(() => Run(loans, reminders));
        Task two = Task.Run(() => Run(reminders, loans));

        Assert.That(Task.WaitAll([one, two], Patience), Is.True,
            "a hang here means the ring was missed");
        Assert.That(reported, Is.EqualTo(1));
    }

    // "Both give up" happens rarely, so the ring is built 100 times.
    [Test]
    public void EveryRingHasExactlyOneVictim()
    {
        int wrong = 0;
        for (int round = 0; round < 100; round++)
        {
            if (RingVictims() != 1)
            {
                wrong++;
            }
        }

        Assert.That(wrong, Is.Zero);
    }

    private static int RingVictims()
    {
        WatchedLock loans = new("loans");
        WatchedLock reminders = new("reminders");
        using Barrier both = new(2);
        int victims = 0;

        void Run(WatchedLock first, WatchedLock second)
        {
            try
            {
                using WatchedLock.Scope a = first.Enter();
                both.SignalAndWait();
                using WatchedLock.Scope b = second.Enter();
            }
            catch (DeadlockException)
            {
                Interlocked.Increment(ref victims);
            }
        }

        Task one = Task.Run(() => Run(loans, reminders));
        Task two = Task.Run(() => Run(reminders, loans));
        bool finished = Task.WaitAll([one, two], Patience);
        return finished ? victims : -1;
    }

    [Test]
    public void PlainContentionReportsNothing()
    {
        WatchedLock loans = new("loans");
        using ManualResetEventSlim held = new();

        Task holder = Task.Run(() =>
        {
            using WatchedLock.Scope scope = loans.Enter();
            held.Set();
            Thread.Sleep(300);
        });
        held.Wait();
        Task waiter = Task.Run(() =>
        {
            using WatchedLock.Scope scope = loans.Enter();
        });

        Assert.That(Task.WaitAll([holder, waiter], Patience), Is.True);
    }

    [Test]
    public void AFixedOrderIsNeverReported()
    {
        WatchedLock loans = new("loans");
        WatchedLock reminders = new("reminders");
        int done = 0;

        Parallel.For(0, 20_000, i =>
        {
            using WatchedLock.Scope a = loans.Enter();
            using WatchedLock.Scope b = reminders.Enter();
            done++;
        });

        Assert.That(done, Is.EqualTo(20_000));
    }

    [Test]
    public void EnteringTwiceOnOneThreadIsRefused()
    {
        WatchedLock loans = new("loans");
        using WatchedLock.Scope scope = loans.Enter();

        Assert.Throws<InvalidOperationException>(() => loans.Enter());
    }
}
