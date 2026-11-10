using System.Diagnostics;

namespace Library.Lending;

// Who has changed which file, read from git log.
public static class BusFactor
{
    private const string Format =
        "--format=@%an%x09%(trailers:key=Co-authored-by,valueonly,separator=%x2C)";

    public static Dictionary<string, HashSet<string>> Authors(
        string repo, bool withCoAuthors, string path = ".")
    {
        Dictionary<string, HashSet<string>> byFile = new(StringComparer.Ordinal);
        string[] who = [];
        foreach (string line in Git(repo, "log", Format, "--name-only", "--", path))
        {
            if (line.StartsWith('@'))
            {
                string[] parts = line[1..].Split('\t');
                IEnumerable<string> co = withCoAuthors && parts.Length > 1
                    ? parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(NameOnly)
                    : [];
                who = [parts[0], .. co];
            }
            else if (line.Length > 0)
            {
                if (!byFile.TryGetValue(line, out HashSet<string>? set))
                {
                    set = new HashSet<string>(StringComparer.Ordinal);
                    byFile[line] = set;
                }

                set.UnionWith(who);
            }
        }

        return byFile;
    }

    public static List<string> SingleAuthorFiles(
        Dictionary<string, HashSet<string>> byFile) =>
        [.. byFile.Where(kv => kv.Value.Count == 1)
            .Select(kv => $"{kv.Key} ({kv.Value.Single()})")
            .Order(StringComparer.Ordinal)];

    // "dev-3 <dev-3@example.org>" becomes "dev-3".
    private static string NameOnly(string trailer) =>
        trailer.Split('<')[0].Trim();

    public static string[] Git(string dir, params string[] args)
    {
        ProcessStartInfo start = new("git") { WorkingDirectory = dir };
        start.RedirectStandardOutput = true;
        foreach (string a in args)
        {
            start.ArgumentList.Add(a);
        }

        using Process git = Process.Start(start)!;
        string output = git.StandardOutput.ReadToEnd();
        git.WaitForExit();
        return output.Split('\n', StringSplitOptions.TrimEntries);
    }
}
