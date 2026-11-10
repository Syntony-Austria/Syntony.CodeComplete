namespace Library.Lending;

// Owns its writer: it creates it, so it closes it.
public sealed class ReminderLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    public ReminderLog(string path) => _writer = new StreamWriter(path);

    public void Write(string line)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(line);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _writer.Dispose();
    }
}

// Borrows its writer: somebody else created it, so somebody else closes it.
public sealed class ReminderLine(TextWriter writer)
{
    public void Write(string line) => writer.WriteLine(line);
}
