namespace Library.Lending;

// Which leaks does ReSharper see? Its own project, outside the tests.
// jb inspectcode LeakProbe/LeakProbe.csproj
//   --severity=SUGGESTION -> nothing here; --severity=HINT -> Leaks only.
// With --profile=LeakProbe/LeakProbe.DotSettings (NotDisposedResource =
//   WARNING) and --severity=WARNING -> Leaks only.
// Closes, Shared and Plain stay silent in all three runs.
public static class LeakProbe
{
    public static void Leaks(string path) =>
        ReceiptFactory.OpenLog(path).WriteLine("x");

    public static void Closes(string path)
    {
        using StreamWriter writer = ReceiptFactory.OpenLog(path);
        writer.WriteLine("x");
    }

    public static void Shared() => ReceiptFactory.SharedLog().WriteLine("x");

    // No factory, no attribute: not reported even with HINT.
    public static void Plain(string path) => new StreamWriter(path).WriteLine("x");
}
