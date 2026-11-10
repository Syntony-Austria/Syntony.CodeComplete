using NUnit.Framework;

namespace Library.Lending;

// On request only:
// dotnet test --filter Name=PrintForecast --logger "console;verbosity=detailed"
public sealed class PrintTests
{
    [Test, Explicit]
    public void PrintForecast()
    {
        int[] weekly = Flow.WeeklyThroughput(
            DemoHistory.Create(), DemoHistory.FirstMonday, DemoHistory.Weeks);
        int[] past = weekly[^12..];
        int[] sim = Forecast.MonteCarlo(past, 30, 10_000, seed: 1);
        TextWriter o = TestContext.Out;
        o.WriteLine("last 12 weeks: " + string.Join(" ", past));
        o.WriteLine($"mean {past.Average():F2} pieces a week");
        o.WriteLine($"by average: {Plan.ByAverage(past, 30)} weeks");
        foreach (double share in new[] { 0.50, 0.85, 0.95 })
        {
            o.WriteLine($"{share:P0} of 10000 runs done within "
                + $"{Forecast.Percentile(sim, share)} weeks");
        }

        foreach (IGrouping<int, int> g in sim.GroupBy(w => w))
        {
            o.WriteLine($"{g.Key,3} weeks {g.Count(),5} "
                + new string('#', (g.Count() + 99) / 100));
        }
    }

    [Test, Explicit]
    public void WriteCsv()
    {
        File.WriteAllLines(Path.Combine(Paths.Project, "Data", "done.csv"),
            ["id,started,finished",
                .. DemoHistory.Create().Select(i => i.ToCsv())]);
    }
}
