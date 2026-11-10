namespace Library.Lending;

public static class LoanService
{
    public static async Task<int> CountAsync()
    {
        await Task.Delay(10);
        return 3;
    }

    public static async Task<int> CountFreeAsync()
    {
        await Task.Delay(10).ConfigureAwait(false);
        return 3;
    }

    // Deliberately wrong: nobody can wait for this and nobody can catch it.
    public static async void SaveAndForget()
    {
        await Task.Yield();
        throw new InvalidOperationException("the disk is full");
    }

    public static async Task SaveAsync()
    {
        await Task.Yield();
        throw new InvalidOperationException("the disk is full");
    }
}
