using NUnit.Framework;

namespace Library.Lending;

public sealed class StormTests
{
    private const int Clients = 1000;

    [Test]
    public void TheSameSeedGivesTheSameStorm()
    {
        StormResult once = RetryStorm.Run(Clients, Spread.Full, seed: 39);
        StormResult again = RetryStorm.Run(Clients, Spread.Full, seed: 39);

        Assert.That(again.PerWindow, Is.EqualTo(once.PerWindow));
    }

    [Test]
    public void WithoutJitterEveryWaveHitsAtOnce()
    {
        StormResult storm = RetryStorm.Run(Clients, Spread.None, seed: 39);

        Assert.That(storm.Peak, Is.EqualTo(Clients));
    }

    [Test]
    public void FullJitterCutsThePeakAndServesMore()
    {
        StormResult none = RetryStorm.Run(Clients, Spread.None, seed: 39);
        StormResult full = RetryStorm.Run(Clients, Spread.Full, seed: 39);

        Assert.That(full.Peak, Is.LessThan(none.Peak / 2));
        Assert.That(full.Served, Is.GreaterThan(none.Served));
    }

    [Test]
    public void PrintTheTable()
    {
        foreach (Spread spread in Enum.GetValues<Spread>())
        {
            foreach (int seed in new[] { 39, 40, 41 })
            {
                StormResult s = RetryStorm.Run(Clients, spread, seed);
                TestContext.Out.WriteLine(
                    $"{spread,-13} seed {seed}: peak {s.Peak}, calls {s.Calls}, "
                    + $"served {s.Served}, last {s.LastServedMs} ms");
            }
        }

        foreach (Spread spread in new[] { Spread.None, Spread.Full })
        {
            StormResult s = RetryStorm.Run(Clients, spread, 39);
            TestContext.Out.WriteLine(
                $"{spread}: " + string.Join(",", s.PerWindow.Take(80)));
        }

        Assert.Pass();
    }
}
