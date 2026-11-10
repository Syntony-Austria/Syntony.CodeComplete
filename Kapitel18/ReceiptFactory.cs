using JetBrains.Annotations;

namespace Library.Lending;

public static class ReceiptFactory
{
    private static readonly StringWriter s_shared = new();

    [MustDisposeResource]
    public static StreamWriter OpenLog(string path) => new(path);

    [MustDisposeResource(false)]
    public static TextWriter SharedLog() => s_shared;
}
