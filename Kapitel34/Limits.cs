namespace Library.Lending;

public static class Limits
{
    // At most "limit" of them at the same time, however many there are.
    public static async Task<int> RunAsync(int count, int limit)
    {
        using SemaphoreSlim gate = new(limit);
        int running = 0;
        int highest = 0;
        Task[] tasks = Enumerable.Range(0, count).Select(async _ =>
        {
            await gate.WaitAsync();
            try
            {
                int now = Interlocked.Increment(ref running);
                InterlockedMax(ref highest, now);
                await Task.Delay(5);
            }
            finally
            {
                Interlocked.Decrement(ref running);
                gate.Release();
            }
        }).ToArray();
        await Task.WhenAll(tasks);
        return highest;
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        do
        {
            current = target;
        }
        while (value > current
            && Interlocked.CompareExchange(ref target, value, current) != current);
    }
}
