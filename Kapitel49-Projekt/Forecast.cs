namespace Library.Lending;

// When will `remaining` pieces be done? Answered from past weeks.
public static class Forecast
{
    // One run draws past weeks at random until enough pieces are done.
    // The result is the sorted number of weeks over all runs.
    public static int[] MonteCarlo(
        int[] pastWeeks, int remaining, int runs, int seed)
    {
        if (pastWeeks.Sum() == 0)
        {
            throw new ArgumentException("no piece finished in these weeks");
        }

        Random random = new(seed);
        int[] weeks = new int[runs];
        for (int run = 0; run < runs; run++)
        {
            for (int done = 0; done < remaining; weeks[run]++)
            {
                done += pastWeeks[random.Next(pastWeeks.Length)];
            }
        }

        Array.Sort(weeks);
        return weeks;
    }

    // The share of runs that were done within the returned week.
    public static int Percentile(int[] sorted, double share) =>
        sorted[(int)Math.Ceiling(share * sorted.Length) - 1];
}
