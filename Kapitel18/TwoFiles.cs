namespace Library.Lending;

public sealed class TwoFilesTangled : IDisposable
{
    private readonly FileStream _first;
    private readonly FileStream _second;

    // Deliberately wrong: if the second open fails, the first stays open.
    public TwoFilesTangled(string first, string second)
    {
        _first = File.OpenRead(first);
        _second = File.OpenRead(second);
    }

    public void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
    }
}

public sealed class TwoFiles : IDisposable
{
    private readonly FileStream _first;
    private readonly FileStream _second;

    public TwoFiles(string first, string second)
    {
        _first = File.OpenRead(first);
        try
        {
            _second = File.OpenRead(second);
        }
        catch
        {
            _first.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
    }
}
