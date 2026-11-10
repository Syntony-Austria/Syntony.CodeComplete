namespace Library.Lending;

// Deliberately wrong. Built only with -p:DisposeProbe=true.
public sealed class HoldsAWriter
{
    private readonly StreamWriter _writer = new("probe.txt");

    public void Write(string line) => _writer.WriteLine(line);
}

public sealed class ForgetsItsWriter : IDisposable
{
    private readonly StreamWriter _writer = new("probe.txt");

    public void Write(string line) => _writer.WriteLine(line);

    public void Dispose()
    {
    }
}

public static class LeaksALocal
{
    public static void Write(string line)
    {
        StreamWriter writer = new("probe.txt");
        writer.WriteLine(line);
    }
}
