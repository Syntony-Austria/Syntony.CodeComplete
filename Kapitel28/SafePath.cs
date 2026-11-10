using System.Text.RegularExpressions;

namespace Library.Lending;

public static class SafePath
{
    // Deliberately wrong: the prefix also fits the folder next door.
    public static bool IsInsideTangled(string root, string name) =>
        Path.GetFullPath(Path.Combine(root, name)).StartsWith(
            Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase);

    public static string? Resolve(string root, string name)
    {
        string baseDirectory = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(Path.Combine(baseDirectory, name));
        return full.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase)
            ? full
            : null;
    }
}

public static class Patterns
{
    // Deliberately wrong: some inputs keep this busy for a very long time.
    public const string Tangled = "^(a+)+$";

    public static readonly TimeSpan Limit = TimeSpan.FromMilliseconds(100);

    public static Regex WithLimit() => new(Tangled, RegexOptions.None, Limit);

    public static Regex Linear() => new(Tangled, RegexOptions.NonBacktracking);
}
