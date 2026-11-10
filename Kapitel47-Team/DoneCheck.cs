namespace Library.Lending;

public sealed record TestRun(int Passed, int Failed, int Skipped);

public sealed record ChangeFacts(
    IReadOnlyList<string> ChangedFiles,
    IReadOnlyList<string> AddedLines,
    TestRun Tests)
{
    // Reads the files and added lines from a unified diff.
    public static ChangeFacts FromDiff(string diff, TestRun tests)
    {
        string[] lines = [.. diff.Split('\n').Select(l => l.TrimEnd('\r'))];
        List<string> files = [.. lines
            .Where(l => l.StartsWith("+++ b/", StringComparison.Ordinal))
            .Select(l => l[6..])];
        List<string> added = [.. lines
            .Where(l => l.StartsWith('+') && !l.StartsWith("+++"))
            .Select(l => l[1..])];
        return new ChangeFacts(files, added, tests);
    }
}

// Checks the ticked boxes of done.md against facts about the change.
public static class DoneCheck
{
    private static readonly Dictionary<string, Func<ChangeFacts, string?>>
        s_checks = new(StringComparer.Ordinal)
        {
            ["tests-changed"] = f => f.ChangedFiles.Any(IsTest)
                ? null : "no test file in the diff",
            ["tests-pass"] = f => f.Tests switch
            {
                { Failed: > 0 } t => $"{t.Failed} failed",
                { Skipped: > 0 } t => $"{t.Skipped} skipped",
                { Passed: 0 } => "no test ran",
                _ => null,
            },
            ["changelog"] = f => f.ChangedFiles.Contains("CHANGELOG.md")
                ? null : "CHANGELOG.md not changed",
            ["no-todo"] = f => f.AddedLines.Any(l => l.Contains("TODO"))
                ? "TODO in an added line" : null,
        };

    public static List<string> Problems(string doneMd, ChangeFacts facts)
    {
        List<string> problems = [];
        foreach (string line in doneMd.Split('\n', StringSplitOptions.TrimEntries))
        {
            bool ticked = line.StartsWith("- [x] ", StringComparison.Ordinal);
            bool open = line.StartsWith("- [ ] ", StringComparison.Ordinal);
            if (!ticked && !open)
            {
                continue;
            }

            string id = line[6..].Split(':')[0];
            if (open && !line.Contains("left out:", StringComparison.Ordinal))
            {
                problems.Add($"{id}: not ticked and no reason given");
            }
            else if (ticked
                && s_checks.TryGetValue(id, out Func<ChangeFacts, string?>? check)
                && check(facts) is string why)
            {
                problems.Add($"{id}: ticked, but {why}");
            }
        }

        return problems;
    }

    private static bool IsTest(string path) =>
        path.EndsWith("Tests.cs", StringComparison.Ordinal);
}
