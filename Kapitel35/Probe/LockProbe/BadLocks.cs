namespace Library.Lending;

// Build this project alone, see LockProbe.csproj for what must come out.
public sealed class BadLocks
{
    private readonly Lock _gate = new();
    private int _count;

    // A stranger can lock the same object.
    public void OnThis()
    {
        lock (this)
        {
            _count++;
        }
    }

    // Every equal string literal is the same object in the whole process.
    public void OnAString()
    {
        lock ("loans")
        {
            _count++;
        }
    }

    // The Type object is shared by everyone who loads the class.
    public void OnAType()
    {
        lock (typeof(BadLocks))
        {
            _count++;
        }
    }

    // Seen as object, the Lock falls back to the old Monitor lock: CS9216.
    public void OnALockAsObject()
    {
        object gate = _gate;
        lock (gate)
        {
            _count++;
        }
    }
}
