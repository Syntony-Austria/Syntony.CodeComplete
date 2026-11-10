using System.Diagnostics;

namespace Library.Lending;

// Finished pieces from git log: every commit "Close <id>" ends one piece.
public static class GitHistory
{
    public static List<(string Id, DateOnly Done)> Read(string repo) =>
        [.. Git(repo, [], "log", "--format=%cs %s")
            .Where(l => l.Contains(" Close "))
            .Select(l => l.Split(' '))
            .Select(p => (p[2], DateOnly.Parse(p[0])))
            .Reverse()];

    public static string[] Git(
        string dir, Dictionary<string, string> env, params string[] args)
    {
        ProcessStartInfo start = new("git") { WorkingDirectory = dir };
        start.RedirectStandardOutput = true;
        foreach (string a in args)
        {
            start.ArgumentList.Add(a);
        }

        foreach (KeyValuePair<string, string> e in env)
        {
            start.Environment[e.Key] = e.Value;
        }

        using Process git = Process.Start(start)!;
        string output = git.StandardOutput.ReadToEnd();
        git.WaitForExit();
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }
}
