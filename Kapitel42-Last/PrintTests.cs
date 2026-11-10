using System.Globalization;
using NUnit.Framework;

namespace Library.Lending;

// Prints the numbers for the book. Run with:
// dotnet test --filter Category=Print --logger "console;verbosity=detailed"
[Category("Print")]
public sealed class PrintTests
{
    private static string F(double v) =>
        v.ToString("0", CultureInfo.InvariantCulture).PadLeft(5);

    private static string Line(string what, LatencySummary s) =>
        $"{what,-22} mean {F(s.Mean)}  p50 {F(s.P50)}  " +
        $"p99 {F(s.P99)}  max {F(s.Max)} ms";

    [Test]
    public void PrintTheMorning()
    {
        double[] one = QueueSimulation.Responses(0.95, 200_000, seed: 42);
        double[] list = FanOut.Slowest(one, 30, 20_000, seed: 7);
        TestContext.Out.WriteLine(Line("one lookup", Latency.Summarize(one)));
        TestContext.Out.WriteLine(Line("book list, 30 titles",
            Latency.Summarize(list)));
        TestContext.Out.WriteLine(
            $"one lookup over 500 ms: {one.Count(t => t > 500)} of {one.Length}");
    }

    [Test]
    public void PrintTheQueueTable()
    {
        foreach (int seed in new[] { 42, 43, 44 })
        {
            foreach (double u in new[] { 0.5, 0.8, 0.9, 0.95 })
            {
                QueueRun r = QueueSimulation.Run(u, 200_000, seed);
                TestContext.Out.WriteLine(
                    $"seed {seed} u {u:0.00} wait {r.MeanWait:0.0} " +
                    Line("", r.Response) +
                    $" L {r.SampledInSystem:0.00} " +
                    $"lW {r.ArrivalsPerMs * r.Response.Mean:0.00}");
            }
        }
    }

    [Test]
    public void PrintTheCurve()
    {
        for (int pct = 10; pct <= 95; pct += 5)
        {
            QueueRun r = QueueSimulation.Run(pct / 100.0, 200_000, 42);
            TestContext.Out.WriteLine(
                $"{pct} {r.Response.Mean:0.0} {r.Response.P99:0.0}");
        }
    }

    [Test]
    public async Task PrintTheLoadTest()
    {
        using SlowCatalog catalog =
            new(slots: 4, TimeSpan.FromMilliseconds(20), TimeProvider.System);
        foreach (int c in new[] { 1, 4, 8, 16, 32, 64 })
        {
            LoadResult r = await LoadTest.RunAsync(catalog.LookupAsync, c,
                Math.Max(256, 16 * c), TimeProvider.System);
            double inFlight = r.PerSecond * r.Latency.Mean / 1000;
            TestContext.Out.WriteLine(
                $"workers {c,3} {r.PerSecond,5:0} /s  p50 {F(r.Latency.P50)}" +
                $"  p99 {F(r.Latency.P99)} ms  lambda*W {inFlight,5:0.0}");
        }
    }
}
