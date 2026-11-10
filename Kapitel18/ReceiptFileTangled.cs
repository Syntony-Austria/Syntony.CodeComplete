namespace Library.Lending;

public static class ReceiptFileTangled
{
    // Deliberately wrong: nobody closes the writer.
    public static void Write(string path, string receipt)
    {
        StreamWriter writer = new(path);
        writer.WriteLine(receipt);
    }

    // Deliberately wrong: the reader is closed before the caller gets it.
    public static StreamReader Open(string path)
    {
        using StreamReader reader = new(path);
        return reader;
    }
}
