namespace Library.Lending;

public sealed record Trial(
    int Cut, int Actual, int ByAverage, int P50, int P85, int P95);

// Forecast at every past week from the weeks before it,
// then compare with what really came after it.
public static class Backtest
{
    public static List<Trial> Run(
        int[] weekly, int remaining, int window, int runs, int seed)
    {
        List<Trial> trials = [];
        for (int cut = window; WeeksUntil(weekly, cut, remaining) > 0; cut++)
        {
            int[] past = weekly[(cut - window)..cut];
            int[] sim = Forecast.MonteCarlo(past, remaining, runs, seed);
            trials.Add(new Trial(cut, WeeksUntil(weekly, cut, remaining),
                Plan.ByAverage(past, remaining),
                Forecast.Percentile(sim, 0.50),
                Forecast.Percentile(sim, 0.85),
                Forecast.Percentile(sim, 0.95)));
        }

        return trials;
    }

    // Share of trials in which the work was done by the forecast week.
    public static double HitRate(List<Trial> trials, Func<Trial, int> week) =>
        trials.Count(t => t.Actual <= week(t)) / (double)trials.Count;

    // Weeks from `from` until `remaining` pieces are done; 0 if never.
    private static int WeeksUntil(int[] weekly, int from, int remaining)
    {
        int done = 0;
        for (int w = from; w < weekly.Length; w++)
        {
            done += weekly[w];
            if (done >= remaining)
            {
                return w - from + 1;
            }
        }

        return 0;
    }
}
