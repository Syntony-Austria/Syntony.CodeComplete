namespace Library.Lending;

public sealed record QueueRun(
    double Utilization, LatencySummary Response, double MeanWait,
    double ArrivalsPerMs, double SampledInSystem);

// One counter, first come first served, a fixed service time and
// random arrivals (exponential gaps). Simulated time, fixed seed.
public static class QueueSimulation
{
    public static QueueRun Run(
        double utilization, int requests, int seed, double serviceMs = 20)
    {
        (double[] arrive, double[] leave) =
            Simulate(utilization, requests, seed, serviceMs);
        LatencySummary summary = Latency.Summarize(Times(arrive, leave));
        double end = arrive[^1];
        return new QueueRun(utilization, summary, summary.Mean - serviceMs,
            requests / end, SampleInSystem(arrive, leave, end));
    }

    public static double[] Responses(
        double utilization, int requests, int seed, double serviceMs = 20)
    {
        (double[] arrive, double[] leave) =
            Simulate(utilization, requests, seed, serviceMs);
        return Times(arrive, leave);
    }

    private static (double[] Arrive, double[] Leave) Simulate(
        double utilization, int requests, int seed, double serviceMs)
    {
        Random random = new(seed);
        double meanGap = serviceMs / utilization;
        double[] arrive = new double[requests];
        double[] leave = new double[requests];
        double now = 0;
        double free = 0;
        for (int i = 0; i < requests; i++)
        {
            now += -meanGap * Math.Log(1 - random.NextDouble());
            double start = Math.Max(now, free);
            free = start + serviceMs;
            arrive[i] = now;
            leave[i] = free;
        }

        return (arrive, leave);
    }

    private static double[] Times(double[] arrive, double[] leave) =>
        [.. leave.Zip(arrive, (l, a) => l - a)];

    // Looks once per simulated millisecond and counts who is inside.
    // This checks L = lambda * W without using W.
    private static double SampleInSystem(
        double[] arrive, double[] leave, double end)
    {
        long inside = 0;
        int samples = 0;
        int arrived = 0;
        int left = 0;
        for (double t = 0; t < end; t += 1, samples++)
        {
            while (arrived < arrive.Length && arrive[arrived] <= t) arrived++;
            while (left < leave.Length && leave[left] <= t) left++;
            inside += arrived - left;
        }

        return (double)inside / samples;
    }
}
