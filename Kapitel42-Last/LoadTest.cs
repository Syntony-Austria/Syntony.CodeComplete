namespace Library.Lending;

// A catalog with a fixed number of slots. Every lookup takes `work`.
public sealed class SlowCatalog(int slots, TimeSpan work, TimeProvider clock)
    : IDisposable
{
    private readonly SemaphoreSlim _slots = new(slots, slots);

    public async Task LookupAsync(CancellationToken cancellationToken)
    {
        await _slots.WaitAsync(cancellationToken);
        try
        {
            await Task.Delay(work, clock, cancellationToken);
        }
        finally
        {
            _slots.Release();
        }
    }

    public void Dispose() => _slots.Dispose();
}

public sealed record LoadResult(
    int Concurrency, double PerSecond, LatencySummary Latency);

public static class LoadTest
{
    // Closed loop: `concurrency` workers, each starts its next request
    // only when the last one has answered.
    public static async Task<LoadResult> RunAsync(
        Func<CancellationToken, Task> call, int concurrency, int requests,
        TimeProvider clock)
    {
        double[] ms = new double[requests];
        ParallelOptions options = new() { MaxDegreeOfParallelism = concurrency };
        long started = clock.GetTimestamp();
        await Parallel.ForEachAsync(Enumerable.Range(0, requests), options,
            async (i, cancellationToken) =>
            {
                long t0 = clock.GetTimestamp();
                await call(cancellationToken);
                ms[i] = clock.GetElapsedTime(t0).TotalMilliseconds;
            });
        double seconds = clock.GetElapsedTime(started).TotalSeconds;
        return new LoadResult(
            concurrency, requests / seconds, Latency.Summarize(ms));
    }
}
