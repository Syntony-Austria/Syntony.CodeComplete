namespace Library.Lending;

public static class Backoff
{
    // first * 2^retry, never above the cap.
    public static TimeSpan Exponential(int retry, TimeSpan first, TimeSpan cap)
    {
        double ms = first.TotalMilliseconds * Math.Pow(2, retry);
        return TimeSpan.FromMilliseconds(Math.Min(ms, cap.TotalMilliseconds));
    }

    // Full jitter: anywhere between zero and the exponential value.
    public static TimeSpan FullJitter(
        int retry, TimeSpan first, TimeSpan cap, Random random) =>
        Exponential(retry, first, cap) * random.NextDouble();

    // Decorrelated jitter: between first and three times the previous wait.
    public static TimeSpan Decorrelated(
        TimeSpan previous, TimeSpan first, TimeSpan cap, Random random)
    {
        double low = first.TotalMilliseconds;
        double high = Math.Max(low, previous.TotalMilliseconds * 3);
        double ms = low + (random.NextDouble() * (high - low));
        return TimeSpan.FromMilliseconds(Math.Min(ms, cap.TotalMilliseconds));
    }
}
