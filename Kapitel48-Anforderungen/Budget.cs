using System.Diagnostics;

namespace Library.Lending;

public static class Budget
{
    // 10,000 loans with a fixed seed, so every run sees the same data.
    public static Loan[] Loans(int count, int seed)
    {
        Random random = new(seed);
        MemberKind[] kinds = Enum.GetValues<MemberKind>();
        DateOnly start = new(2026, 1, 1);
        return [.. Enumerable.Range(0, count).Select(i => new Loan(
            "m-" + i, kinds[random.Next(kinds.Length)], random.Next(10) == 0,
            start.AddDays(random.Next(365))))];
    }

    // The work LEND-010 is about: one fee for every loan.
    public static long FeesForAll(Loan[] loans, DateOnly today)
    {
        long sum = 0;
        foreach (Loan loan in loans)
        {
            int late = Math.Max(0, today.DayNumber - loan.Due.DayNumber);
            sum += LoanFee.InCents(late, loan.Kind, loan.Reference);
        }

        return sum;
    }

    // Milliseconds per run, measured `runs` times.
    public static List<double> Measure(int runs, Func<long> work)
    {
        List<double> times = [];
        for (int run = 0; run < runs; run++)
        {
            long begin = Stopwatch.GetTimestamp();
            GC.KeepAlive(work());
            times.Add(Stopwatch.GetElapsedTime(begin).TotalMilliseconds);
        }

        return times;
    }

    // Nearest rank: the time that 95 % of the runs did not exceed.
    public static double Percentile(List<double> times, double p)
    {
        double[] sorted = [.. times.Order()];
        int rank = (int)Math.Ceiling(p / 100 * sorted.Length);
        return sorted[Math.Max(0, rank - 1)];
    }
}
