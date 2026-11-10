using NUnit.Framework;

namespace Library.Lending;

public sealed class UpdateTests
{
    private string side = "";
    private string install = "";

    [SetUp]
    public void Folders()
    {
        string root = Path.Combine(Path.GetTempPath(), "k40-" + Guid.NewGuid());
        side = Directory.CreateDirectory(Path.Combine(root, "side")).FullName;
        install = Directory.CreateDirectory(Path.Combine(root, "app")).FullName;
        File.WriteAllText(Path.Combine(install, "Lending.dll"), "1.3.1");
    }

    [TearDown]
    public void Clean() =>
        Directory.Delete(Path.GetDirectoryName(side)!, recursive: true);

    public static Manifest For(string folder, Version version) =>
        new("Library", "win", version,
            Directory.GetFiles(folder).ToDictionary(
                path => Path.GetFileName(path), UpdateGate.Sha256Of));

    private Manifest Package(string content, Version version)
    {
        File.WriteAllText(Path.Combine(side, "Lending.dll"), content);
        return For(side, version);
    }

    private string Installed() =>
        File.ReadAllText(Path.Combine(install, "Lending.dll"));

    [Test]
    public void AGoodPackageIsInstalled()
    {
        Manifest manifest = Package("1.3.2", new(1, 3, 2));

        UpdateOutcome outcome = UpdateGate.Apply(
            manifest, "Library", "win", new(1, 3, 1), side, install);

        Assert.That(outcome.Applied, Is.True);
        Assert.That(Installed(), Is.EqualTo("1.3.2"));
    }

    [Test]
    public void ATamperedFileNeverReachesTheInstallation()
    {
        Manifest manifest = Package("1.3.2", new(1, 3, 2));
        File.WriteAllText(Path.Combine(side, "Lending.dll"), "1.3.2 + extra");

        UpdateOutcome outcome = UpdateGate.Apply(
            manifest, "Library", "win", new(1, 3, 1), side, install);

        Assert.That(outcome.Reason,
            Is.EqualTo("Lending.dll does not match the manifest"));
        Assert.That(Installed(), Is.EqualTo("1.3.1"));
    }

    [TestCase(1, 3, 0, "1.3.0 is not newer than 1.3.1")]
    [TestCase(1, 3, 1, "1.3.1 is not newer than 1.3.1")]
    public void AnOlderOrEqualVersionIsRefused(
        int major, int minor, int patch, string reason)
    {
        Manifest manifest = Package("other code", new(major, minor, patch));

        UpdateOutcome outcome = UpdateGate.Apply(
            manifest, "Library", "win", new(1, 3, 1), side, install);

        Assert.That(outcome.Reason, Is.EqualTo(reason));
        Assert.That(Installed(), Is.EqualTo("1.3.1"));
    }

    [Test]
    public void ANameThatLeavesTheFolderIsRefused()
    {
        string package = Directory.CreateDirectory(Path.Combine(side, "pkg")).FullName;
        string name = Path.Combine("..", "evil.dll");
        File.WriteAllText(Path.Combine(package, name), "evil");
        Manifest manifest = new("Library", "win", new(1, 3, 2),
            new Dictionary<string, string>
            {
                [name] = UpdateGate.Sha256Of(Path.Combine(package, name)),
            });

        UpdateOutcome outcome = UpdateGate.Apply(
            manifest, "Library", "win", new(1, 3, 1), package, install);

        Assert.That(outcome.Reason, Is.EqualTo($"{name} is not a plain file name"));
        Assert.That(File.Exists(Path.Combine(install, name)), Is.False);
    }

    [Test]
    public void TheNaiveOrderLeavesTheTamperedFileInPlace()
    {
        Manifest manifest = Package("1.3.2", new(1, 3, 2));
        File.WriteAllText(Path.Combine(side, "Lending.dll"), "1.3.2 + extra");

        UpdateOutcome outcome = NaiveUpdate.Apply(manifest, side, install);

        Assert.That(outcome.Applied, Is.False);
        Assert.That(Installed(), Is.EqualTo("1.3.2 + extra"));
    }
}
