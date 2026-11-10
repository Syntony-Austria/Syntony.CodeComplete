namespace Library.Lending;

public static class Waiting
{
    // Deliberately wrong: every call keeps a thread busy doing nothing.
    public static Task BlockingAsync(int milliseconds) =>
        Task.Run(() => Thread.Sleep(milliseconds));

    // No thread waits here. A timer finishes the task later.
    public static Task FreeAsync(int milliseconds) => Task.Delay(milliseconds);
}
