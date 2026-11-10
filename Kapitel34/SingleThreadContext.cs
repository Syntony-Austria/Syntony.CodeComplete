namespace Library.Lending;

// What a user interface has: one thread and a queue for that thread.
public sealed class SingleThreadContext : SynchronizationContext
{
    private readonly Queue<(SendOrPostCallback Work, object? State)> _queue = new();
    private readonly object _gate = new();

    public List<Exception> Crashes { get; } = [];

    public override void Post(SendOrPostCallback d, object? state)
    {
        lock (_gate)
        {
            _queue.Enqueue((d, state));
        }
    }

    // Runs what is queued until nothing has come for the given time.
    public void Pump(TimeSpan quiet)
    {
        DateTime last = DateTime.UtcNow;
        while (DateTime.UtcNow - last < quiet)
        {
            (SendOrPostCallback Work, object? State) next;
            lock (_gate)
            {
                if (_queue.Count == 0)
                {
                    continue;
                }

                next = _queue.Dequeue();
            }

            try
            {
                next.Work(next.State);
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                Crashes.Add(ex);
            }

            last = DateTime.UtcNow;
        }
    }
}
