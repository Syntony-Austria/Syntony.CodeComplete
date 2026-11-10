using System.Text.RegularExpressions;

namespace Library.Lending;

public sealed record Finding(string File, int Line, string Text)
{
    // No line number: inserting a line above must not make it new.
    public string Key => $"{File}|{Text}";
}

// Team rule: overdue fees are computed only in FeeSchedule.
public static partial class FeeRuleGate
{
    private const string Home = "FeeSchedule.cs";

    public static List<Finding> Find(string sourceDir)
    {
        List<Finding> found = [];
        foreach (string path in Directory.GetFiles(sourceDir, "*.cs")
            .Order(StringComparer.Ordinal))
        {
            string file = Path.GetFileName(path);
            if (file == Home)
            {
                continue;
            }

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                if (FeeArithmetic().IsMatch(lines[i]))
                {
                    found.Add(new Finding(file, i + 1, lines[i].Trim()));
                }
            }
        }

        return found;
    }

    // Subtracts the three grace days or multiplies by 25 cents.
    [GeneratedRegex(@"\b\w+\s*-\s*3\b|\*\s*25\b")]
    private static partial Regex FeeArithmetic();
}
