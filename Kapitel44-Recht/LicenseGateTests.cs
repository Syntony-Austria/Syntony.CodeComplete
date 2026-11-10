using NUnit.Framework;

namespace Library.Lending;

public sealed class LicenseGateTests
{
    private static readonly string Assets = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "obj", "project.assets.json");

    private static readonly HashSet<string> Allowed = ["MIT", "Apache-2.0"];

    private static readonly Dictionary<string, string> Reviewed = [];

    [Test]
    public void ReportTheLicenseOfEveryRestoredPackage()
    {
        List<PackageLicense> packages = LicenseGate.Read(Assets);

        foreach (PackageLicense p in packages)
        {
            TestContext.Out.WriteLine(
                $"{p.Package}\t{p.Kind}\t{p.Value}\tnotices={p.NoticeFiles}");
        }

        Assert.That(packages, Is.Not.Empty);
    }

    [Test]
    public void TheGatePassesWithTheListOfThisProject()
    {
        List<Finding> findings = LicenseGate.Check(
            LicenseGate.Read(Assets), Allowed, Reviewed);

        Assert.That(findings, Is.Empty, string.Join("\n", findings));
    }

    [Test]
    public void WithoutMitOnTheListTheGateNamesThePackages()
    {
        HashSet<string> apacheOnly = ["Apache-2.0"];

        List<Finding> findings = LicenseGate.Check(
            LicenseGate.Read(Assets), apacheOnly, Reviewed);

        Assert.That(findings.Select(f => f.Package),
            Has.Some.StartsWith("NUnit/"));
        Assert.That(findings.Select(f => f.Code),
            Is.All.EqualTo("license.not_allowed"));
    }

    [TestCase("<license type=\"expression\">MIT OR GPL-3.0-only</license>",
        "license.not_allowed")]
    [TestCase("<license type=\"file\">LICENSE.txt</license>",
        "license.file_needs_review")]
    [TestCase("<licenseUrl>https://example.org/license</licenseUrl>",
        "license.url_only")]
    [TestCase("", "license.missing")]
    public void WhatTheListDoesNotNameIsAFinding(string element, string code)
    {
        string nuspec = Path.Combine(
            Directory.CreateTempSubdirectory("k44-").FullName, "demo.nuspec");
        File.WriteAllText(nuspec,
            "<package><metadata><id>Demo</id>" + element + "</metadata></package>");

        List<Finding> findings = LicenseGate.Check(
            [LicenseGate.FromNuspec("Demo/1.0.0", nuspec)], Allowed, Reviewed);

        Assert.That(findings.Single().Code, Is.EqualTo(code));
    }

    [Test]
    public void AReviewedPackagePassesWithItsReason()
    {
        PackageLicense url = new("Old/1.0.0", LicenseKind.Url, "https://x", 0);
        Dictionary<string, string> reviewed = new()
        {
            ["Old/1.0.0"] = "MIT text read at the URL on 2026-11-08",
        };

        Assert.That(LicenseGate.Check([url], Allowed, reviewed), Is.Empty);
    }
}
