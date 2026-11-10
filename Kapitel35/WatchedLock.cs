namespace Library.Lending;

public sealed class DeadlockException(string message) : Exception(message);

// A lock that notices a ring of waiters instead of waiting forever.
// Who waits for which lock is a graph; a cycle in it is the deadlock.
public sealed class WatchedLock(string name)
{
    private static readonly TimeSpan Slice = TimeSpan.FromMilliseconds(50);
    private static readonly Lock Graph = new();
    private static readonly Dictionary<int, WatchedLock> WaitsFor = [];

    private readonly Lock _gate = new();
    private int _owner;

    public string Name { get; } = name;

    public Scope Enter()
    {
        int me = Environment.CurrentManagedThreadId;
        if (Volatile.Read(ref _owner) == me)
        {
            throw new InvalidOperationException($"{Name} is not reentrant.");
        }

        if (!_gate.TryEnter())
        {
            WaitWatched(me);
        }

        Volatile.Write(ref _owner, me);
        return new Scope(this);
    }

    // Only contention pays for the graph: the fast path above never touches it.
    private void WaitWatched(int me)
    {
        lock (Graph)
        {
            WaitsFor[me] = this;
        }

        try
        {
            while (!_gate.TryEnter(Slice))
            {
                lock (Graph)
                {
                    ThrowIfRing(me);
                }
            }
        }
        finally
        {
            lock (Graph)
            {
                WaitsFor.Remove(me);
            }
        }
    }

    // Follow owner -> lock it waits for -> its owner ... back to me?
    private static void ThrowIfRing(int me)
    {
        WatchedLock wanted = WaitsFor[me];
        for (int step = 0; step <= WaitsFor.Count; step++)
        {
            int owner = Volatile.Read(ref wanted._owner);
            if (owner == me)
            {
                WaitsFor.Remove(me); // one victim is enough
                throw new DeadlockException($"Ring of waiters at {wanted.Name}.");
            }

            if (owner == 0 || !WaitsFor.TryGetValue(owner, out wanted!))
            {
                return;
            }
        }
    }

    private void Exit()
    {
        Volatile.Write(ref _owner, 0);
        _gate.Exit();
    }

    // A class, not a struct: a copy of the handle cannot release twice.
    public sealed class Scope(WatchedLock owner) : IDisposable
    {
        private WatchedLock? _owner = owner;

        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.Exit();
    }
}
