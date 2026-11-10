using System.Text.Json;
using System.Xml.Linq;

namespace Library.Lending;

public enum LicenseKind { Expression, File, Url, None }

public sealed record PackageLicense(
    string Package, LicenseKind Kind, string Value, int NoticeFiles);

public sealed record Finding(string Package, string Code, string Detail);

// Reads the license of every restored package and checks it against a list.
public static class LicenseGate
{
    public static List<PackageLicense> Read(string assetsFile)
    {
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(assetsFile));
        JsonElement root = doc.RootElement;
        string cache = root.GetProperty("packageFolders")
            .EnumerateObject().First().Name;
        List<PackageLicense> result = [];
        JsonElement libraries = root.GetProperty("libraries");
        foreach (JsonProperty lib in libraries.EnumerateObject())
        {
            if (lib.Value.GetProperty("type").GetString() != "package")
            {
                continue;
            }

            string[] files = lib.Value.GetProperty("files").EnumerateArray()
                .Select(f => f.GetString() ?? "").ToArray();
            string nuspec = Path.Combine(cache,
                lib.Value.GetProperty("path").GetString() ?? "",
                files.First(f => f.EndsWith(".nuspec", StringComparison.Ordinal)));
            int notices = files.Count(f =>
                f.Contains("NOTICE", StringComparison.OrdinalIgnoreCase));
            result.Add(FromNuspec(lib.Name, nuspec, notices));
        }

        return result;
    }

    public static PackageLicense FromNuspec(
        string package, string nuspec, int noticeFiles = 0)
    {
        XElement meta = XDocument.Load(nuspec).Root!.Elements()
            .First(e => e.Name.LocalName == "metadata");
        XElement? license = Child(meta, "license");
        if (license is not null)
        {
            LicenseKind kind = (string?)license.Attribute("type") == "expression"
                ? LicenseKind.Expression
                : LicenseKind.File;
            return new(package, kind, license.Value.Trim(), noticeFiles);
        }

        XElement? url = Child(meta, "licenseUrl");
        return url is null
            ? new(package, LicenseKind.None, "", noticeFiles)
            : new(package, LicenseKind.Url, url.Value.Trim(), noticeFiles);
    }

    // Fail closed: only an exact entry of the list passes, or a human review.
    public static List<Finding> Check(IEnumerable<PackageLicense> packages,
        IReadOnlySet<string> allowed,
        IReadOnlyDictionary<string, string> reviewed)
    {
        List<Finding> findings = [];
        foreach (PackageLicense p in packages)
        {
            if (reviewed.ContainsKey(p.Package)
                || (p.Kind == LicenseKind.Expression && allowed.Contains(p.Value)))
            {
                continue;
            }

            string code = p.Kind switch
            {
                LicenseKind.Expression => "license.not_allowed",
                LicenseKind.File => "license.file_needs_review",
                LicenseKind.Url => "license.url_only",
                _ => "license.missing",
            };
            findings.Add(new Finding(p.Package, code, p.Value));
        }

        return findings;
    }

    private static XElement? Child(XElement parent, string name) =>
        parent.Elements().FirstOrDefault(e => e.Name.LocalName == name);
}
