using System.Xml.Linq;

namespace Library.Lending.Tests;

// Checks that CHANGELOG.md and README.md keep up with the project.
internal static class ReleaseDocsGate
{
    private static readonly string[] s_kinds =
        ["Added", "Changed", "Deprecated", "Removed", "Fixed", "Security"];

    private static readonly string[] s_readmeSections =
        ["## Purpose", "## Status", "## First five minutes", "## Where next"];

    public static string ProjectVersion(string root)
    {
        string project = Path.Combine(
            root, "src", "Library.Lending", "Library.Lending.csproj");
        return XDocument.Load(project).Descendants("Version").Single().Value;
    }

    public static List<string> ChangelogProblems(string root)
    {
        string[] lines = File.ReadAllLines(Path.Combine(root, "CHANGELOG.md"));
        string version = ProjectVersion(root);
        List<string> problems = [];

        string entry = $"## [{version}] - ";
        if (!lines.Any(l => l.StartsWith(entry, StringComparison.Ordinal)))
        {
            problems.Add($"CHANGELOG.md: no entry '## [{version}] - <date>'");
        }

        IEnumerable<string> kinds = lines
            .Where(l => l.StartsWith("### ", StringComparison.Ordinal))
            .Select(l => l[4..]);
        foreach (string kind in kinds.Where(k => !s_kinds.Contains(k)))
        {
            problems.Add($"CHANGELOG.md: unknown kind '{kind}'");
        }

        return problems;
    }

    public static List<string> ReadmeProblems(string root)
    {
        string[] lines = File.ReadAllLines(Path.Combine(root, "README.md"));
        return s_readmeSections
            .Where(s => !lines.Contains(s, StringComparer.Ordinal))
            .Select(s => $"README.md: missing '{s}'")
            .ToList();
    }
}
