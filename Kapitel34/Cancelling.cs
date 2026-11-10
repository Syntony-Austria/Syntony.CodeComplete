namespace Library.Lending;

public sealed class ReminderRun(TimeProvider clock)
{
    public int Sent { get; private set; }

    // Deliberately wrong: takes the token and never looks at it.
    public async Task IgnoresTokenAsync(int members, CancellationToken token)
    {
        for (int i = 0; i < members; i++)
        {
            await Task.Yield();
            Sent++;
        }
    }

    public async Task SendAllAsync(int members, CancellationToken token)
    {
        for (int i = 0; i < members; i++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromSeconds(1), clock, token);
            Sent++;
        }
    }
}
