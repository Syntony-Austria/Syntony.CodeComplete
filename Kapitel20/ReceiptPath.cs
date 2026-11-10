namespace Library.Lending;

public static class ReceiptPathTangled
{
    // Deliberately wrong: the name comes from outside.
    public static string For(string baseFolder, string fileName) =>
        Path.Combine(baseFolder, fileName);
}

public static class ReceiptPath
{
    public static string? For(string baseFolder, string fileName)
    {
        string root = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(baseFolder)) + Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(Path.Combine(root, fileName));
        bool isInside = full.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        return isInside ? full : null;
    }
}
