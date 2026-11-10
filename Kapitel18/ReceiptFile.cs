namespace Library.Lending;

public static class ReceiptFile
{
    public static void Write(string path, string receipt)
    {
        using StreamWriter writer = new(path);
        writer.WriteLine(receipt);
    }

    // The caller owns what this returns and has to dispose it.
    public static StreamReader Open(string path) => new(path);

    public static IEnumerable<string> ReadLines(string path)
    {
        using StreamReader reader = new(path);
        while (reader.ReadLine() is string line)
        {
            yield return line;
        }
    }
}
