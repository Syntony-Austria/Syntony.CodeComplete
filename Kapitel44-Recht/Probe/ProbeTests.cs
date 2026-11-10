using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Library.Lending;

// Deliberately red. Build and run only with: dotnet test -p:Probe=true
// Each probe expects the opposite of what the real test has measured.
public sealed class ProbeTests
{
    private const string Email = "m1@example.org";

    private static readonly Dictionary<string, DataClass> Fields = new()
    {
        ["Email"] = DataClass.Personal,
    };

    [Test]
    public void TheMaskAlsoCatchesInterpolatedText()
    {
        ListSink sink = new();
        ILogger logger = new MaskingLogger(sink, Fields, maskUnknown: true);

        logger.LogInformation($"Renewed for <{Email}>");

        Assert.That(sink.All(), Does.Not.Contain(Email));
    }

    [Test]
    public void WithoutAMaskTheEmailStaysOut()
    {
        ListSink sink = new();

        RenewalAudit.Renewed(sink, "Title", "Name M1", Email, 1);

        Assert.That(sink.All(), Does.Not.Contain(Email));
    }

    [Test]
    public void APlainHashCannotBeTracedBack()
    {
        string seen = Pseudonyms.PlainHash("m4711@example.org");

        bool found = Enumerable.Range(0, 10_000)
            .Any(i => Pseudonyms.PlainHash($"m{i}@example.org") == seen);

        Assert.That(found, Is.False);
    }

    [Test]
    public void TheGatePassesWithoutMitOnTheList()
    {
        string assets = Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "obj", "project.assets.json");
        HashSet<string> apacheOnly = ["Apache-2.0"];

        List<Finding> findings = LicenseGate.Check(
            LicenseGate.Read(assets), apacheOnly, new Dictionary<string, string>());

        Assert.That(findings, Is.Empty);
    }
}
