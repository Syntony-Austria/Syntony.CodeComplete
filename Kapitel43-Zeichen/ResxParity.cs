using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Library.Lending;

// Every text in both files, with the same placeholders.
public static partial class ResxParity
{
    public static IReadOnlyList<string> Check(string neutralPath,
        string translatedPath)
    {
        Dictionary<string, string> neutral = Read(neutralPath);
        Dictionary<string, string> translated = Read(translatedPath);
        List<string> problems = [];
        foreach (string key in neutral.Keys.Except(translated.Keys))
        {
            problems.Add($"missing in translation: {key}");
        }

        foreach (string key in translated.Keys.Except(neutral.Keys))
        {
            problems.Add($"only in translation: {key}");
        }

        foreach (string key in neutral.Keys.Intersect(translated.Keys))
        {
            string a = Holes(neutral[key]);
            string b = Holes(translated[key]);
            if (a != b)
            {
                problems.Add($"placeholders differ in {key}: {a} vs {b}");
            }
        }

        return problems;
    }

    private static Dictionary<string, string> Read(string path) =>
        XDocument.Load(path).Root!.Elements("data").ToDictionary(
            d => (string)d.Attribute("name")!,
            d => (string?)d.Element("value") ?? "",
            StringComparer.Ordinal);

    // "{0}: {1} days, fee {3:C}" -> "{0,1,3}"
    private static string Holes(string text) =>
        "{" + string.Join(',', Placeholder().Matches(text)
            .Select(m => int.Parse(m.Groups[1].Value,
                System.Globalization.CultureInfo.InvariantCulture))
            .Distinct().Order()) + "}";

    [GeneratedRegex(@"\{(\d+)[^}]*\}")]
    private static partial Regex Placeholder();
}
