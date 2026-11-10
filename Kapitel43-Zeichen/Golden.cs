namespace Library.Lending;

// Snapshot without a library: compare with a file in the repository.
// UPDATE_GOLDEN=1 dotnet test rewrites the files; review the diff.
public static class Golden
{
    public static string? Compare(string actual, string path)
    {
        if (Environment.GetEnvironmentVariable("UPDATE_GOLDEN") == "1")
        {
            File.WriteAllText(path, actual);
            return null;
        }

        // Git may write \r\n on Windows: compare lines, not line ends.
        string[] want = File.ReadAllText(path).ReplaceLineEndings("\n").Split('\n');
        string[] got = actual.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < Math.Max(want.Length, got.Length); i++)
        {
            string w = i < want.Length ? want[i] : "<end>";
            string g = i < got.Length ? got[i] : "<end>";
            if (w != g)
            {
                return $"{Path.GetFileName(path)} line {i + 1}\n" +
                       $"  want: {w}\n  got:  {g}";
            }
        }

        return null;
    }
}
