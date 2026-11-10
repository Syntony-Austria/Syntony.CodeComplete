namespace Library.Lending;

public static class DelayedTangled
{
    // Deliberately wrong: the query runs after the reader is closed.
    public static IEnumerable<string> LongLines(string path)
    {
        using StreamReader reader = new(path);
        return Lines(reader).Where(line => line.Length > 3);
    }

    // Deliberately wrong: the task keeps running while the reader closes.
    public static Task<string> ReadAll(string path)
    {
        using StreamReader reader = new(path);
        return reader.ReadToEndAsync();
    }

    public static IEnumerable<string> Lines(StreamReader reader)
    {
        while (reader.ReadLine() is string line)
        {
            yield return line;
        }
    }
}

public static class Delayed
{
    public static IReadOnlyList<string> LongLines(string path)
    {
        using StreamReader reader = new(path);
        return [.. DelayedTangled.Lines(reader).Where(line => line.Length > 3)];
    }

    public static async Task<string> ReadAll(string path)
    {
        using StreamReader reader = new(path);
        return await reader.ReadToEndAsync();
    }

    public static async Task WriteAll(string path, string text)
    {
        await using StreamWriter writer = new(path);
        await writer.WriteAsync(text);
    }
}
