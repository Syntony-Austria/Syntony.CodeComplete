namespace Library.Lending;

public enum Spread
{
    None,
    Full,
    Decorrelated,
}

public sealed record StormResult(
    int Peak, int Calls, int Served, int LastServedMs, int[] PerWindow);

// Many clients, one service that is down at first and then has a capacity.
// Pure arithmetic on a simulated clock: same seed, same numbers.
public static class RetryStorm
{
    public const int WindowMs = 100;

    public static StormResult Run(
        int clients, Spread spread, int seed,
        int outageMs = 2000, int capacityPerWindow = 100, int maxAttempts = 8)
    {
        TimeSpan first = TimeSpan.FromMilliseconds(100);
        TimeSpan cap = TimeSpan.FromSeconds(10);
        Random random = new(seed);
        PriorityQueue<(int Client, int Attempt, TimeSpan Wait), (int, int)> due =
            new();
        for (int client = 0; client < clients; client++)
        {
            due.Enqueue((client, 0, first), (0, client));
        }

        Dictionary<int, int> perWindow = [];
        int calls = 0, served = 0, last = 0;
        while (due.TryDequeue(
                   out (int Client, int Attempt, TimeSpan Wait) next,
                   out (int Time, int) when))
        {
            calls++;
            int window = when.Time / WindowMs;
            int load = perWindow.GetValueOrDefault(window) + 1;
            perWindow[window] = load;
            if (when.Time >= outageMs && load <= capacityPerWindow)
            {
                (served, last) = (served + 1, when.Time);
                continue;
            }

            if (next.Attempt + 1 >= maxAttempts)
            {
                continue;
            }

            TimeSpan wait = spread switch
            {
                Spread.None => Backoff.Exponential(next.Attempt, first, cap),
                Spread.Full => Backoff.FullJitter(next.Attempt, first, cap, random),
                _ => Backoff.Decorrelated(next.Wait, first, cap, random),
            };
            int at = when.Time + (int)wait.TotalMilliseconds;
            due.Enqueue((next.Client, next.Attempt + 1, wait), (at, next.Client));
        }

        int[] series = new int[perWindow.Keys.Max() + 1];
        foreach (KeyValuePair<int, int> pair in perWindow)
        {
            series[pair.Key] = pair.Value;
        }

        // The peak that matters: once the service is back and has a limit.
        int peak = series.Skip(outageMs / WindowMs).DefaultIfEmpty().Max();
        return new(peak, calls, served, last, series);
    }
}
