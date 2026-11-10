using System.Text.RegularExpressions;

namespace Library.Lending;

public sealed record Requirement(string Id, string Text);

public sealed record Example(
    string Id, int DaysLate, MemberKind Kind, bool Reference, int Cents);

public static partial class RequirementsFile
{
    [GeneratedRegex(@"^(LEND-\d{3}): (.+)$")]
    private static partial Regex Line();

    // "LEND-001: text", continued on lines that start with two spaces.
    public static List<Requirement> Requirements(string[] lines)
    {
        List<Requirement> found = [];
        foreach (string line in lines.TakeWhile(l => !l.StartsWith("## ")))
        {
            Match m = Line().Match(line);
            if (m.Success)
            {
                found.Add(new Requirement(m.Groups[1].Value, m.Groups[2].Value));
            }
            else if (line.StartsWith("  ") && found.Count > 0)
            {
                Requirement last = found[^1];
                found[^1] = last with { Text = last.Text + " " + line.Trim() };
            }
        }

        return found;
    }

    // Table rows under "## Examples": | ID | days | member | ref | cents |
    public static List<Example> Examples(string[] lines)
    {
        List<Example> found = [];
        foreach (string line in lines.SkipWhile(l => l != "## Examples"))
        {
            string[] cells = [.. line.Split('|').Select(c => c.Trim())];
            if (cells.Length == 7 && cells[1].StartsWith("LEND-"))
            {
                found.Add(new Example(cells[1], int.Parse(cells[2]),
                    Enum.Parse<MemberKind>(cells[3]), cells[4] == "yes",
                    int.Parse(cells[5])));
            }
        }

        return found;
    }
}
