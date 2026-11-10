namespace Library.Lending;

public sealed class Counters
{
    private readonly Lock _gate = new();
    private int _plain;
    private int _atomic;
    private int _locked;

    public int Plain => _plain;

    public int Atomic => _atomic;

    public int Locked => _locked;

    // Deliberately wrong: read, add, write are three steps.
    public void AddPlain() => _plain++;

    public void AddAtomic() => Interlocked.Increment(ref _atomic);

    public void AddLocked()
    {
        lock (_gate)
        {
            _locked++;
        }
    }
}
