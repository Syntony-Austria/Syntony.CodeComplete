namespace Library.Lending;

public sealed class FailingDispose : IDisposable
{
    public void Dispose() => throw new IOException("Buffer could not be written.");
}

public static class Covering
{
    public static void Work()
    {
        using FailingDispose resource = new();
        throw new InvalidOperationException("The real problem.");
    }
}
