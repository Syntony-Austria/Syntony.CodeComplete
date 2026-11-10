namespace Library.Lending;

public static class Tail
{
    // Chance that at least one of `fanOut` parallel calls is slow,
    // if each one is slow with chance `slow`, independently.
    public static double AnySlow(double slow, int fanOut) =>
        1 - Math.Pow(1 - slow, fanOut);
}

public static class FanOut
{
    // A request that asks `fanOut` lookups at once and waits for the
    // slowest. Each lookup time is drawn at random from `pool`.
    public static double[] Slowest(
        IReadOnlyList<double> pool, int fanOut, int requests, int seed)
    {
        Random random = new(seed);
        double[] slowest = new double[requests];
        for (int r = 0; r < requests; r++)
        {
            for (int i = 0; i < fanOut; i++)
            {
                double one = pool[random.Next(pool.Count)];
                slowest[r] = Math.Max(slowest[r], one);
            }
        }

        return slowest;
    }
}
