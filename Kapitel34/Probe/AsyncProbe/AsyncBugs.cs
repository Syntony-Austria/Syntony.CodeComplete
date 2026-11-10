namespace Library.Lending;

// Eight known async bugs. Build this project alone, see AsyncProbe.csproj.
public static class AsyncBugs
{
    private static async Task<int> CountAsync(CancellationToken token)
    {
        await Task.Delay(10, token);
        return 3;
    }

    // 1. Blocking with .Result.
    public static int BlockWithResult() => CountAsync(default).Result;

    // 2. Blocking with GetAwaiter().GetResult().
    public static int BlockWithGetResult() =>
        CountAsync(default).GetAwaiter().GetResult();

    // 3. async void outside an event handler.
    public static async void SaveAndForget() => await Task.Delay(10);

    // 4. A task without await inside an async method.
    public static async Task ForgetTheAwaitAsync()
    {
        Task.Delay(10);
        await Task.Yield();
    }

    // 5. A discarded task: the underscore silences the only warning.
    public static async Task DiscardTheTaskAsync()
    {
        _ = Task.Delay(10);
        await Task.Yield();
    }

    // 6. No ConfigureAwait in library code.
    public static async Task<int> NoConfigureAwaitAsync(CancellationToken token) =>
        await CountAsync(token);

    // 7. Thread.Sleep inside an async method.
    public static async Task SleepInsideAsync()
    {
        Thread.Sleep(10);
        await Task.Yield();
    }

    // 8. The token is taken and not passed on.
    public static async Task DropTheTokenAsync(CancellationToken token) =>
        await Task.Delay(10);
}
