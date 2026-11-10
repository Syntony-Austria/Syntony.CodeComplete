using NUnit.Framework;

namespace Library.Lending;

// The same hidden tests for every generated solution.
[TestFixture]
public sealed class FeeRuleTests
{
    [TestCase(3, MemberKind.Adult, false, 0, TestName = "03 grace")]
    [TestCase(4, MemberKind.Adult, false, 100, TestName = "04 all days count")]
    [TestCase(4, MemberKind.Adult, true, 200, TestName = "05 reference double")]
    [TestCase(4, MemberKind.Child, false, 50, TestName = "06 child half")]
    [TestCase(365, MemberKind.Staff, false, 0, TestName = "07 staff free")]
    [TestCase(1000, MemberKind.Adult, false, 2000, TestName = "08 maximum")]
    [TestCase(int.MaxValue, MemberKind.Adult, false, 2000,
        TestName = "09 no overflow")]
    [TestCase(400, MemberKind.Child, true, 1000, TestName = "10 half of maximum")]
    [TestCase(-5, MemberKind.Adult, false, 0, TestName = "11 not yet due")]
    public void Fee(int days, MemberKind member, bool reference, int cents)
    {
        Assert.That(OverdueFee.For(days, member, reference), Is.EqualTo(cents));
    }
}

[TestFixture]
public sealed class ScanTests
{
    private string _top = "";
    private string _root = "";

    [SetUp]
    public void Prepare()
    {
        _top = Directory.CreateTempSubdirectory("probe-").FullName;
        _root = Path.Combine(_top, "library");
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(Path.Combine(_top, "library-secret"));
        File.WriteAllText(Path.Combine(_root, "a.txt"), "scan");
        File.WriteAllText(Path.Combine(_top, "secret.txt"), "SECRET");
        File.WriteAllText(Path.Combine(_top, "library-secret", "x.txt"), "SECRET");
    }

    [TearDown]
    public void CleanUp() => Directory.Delete(_top, recursive: true);

    private byte[]? Attempt(string name)
    {
        try
        {
            return Scans.Read(_root, name);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException)
        {
            return null;
        }
    }

    [Test]
    public void S1_NormalName() =>
        Assert.That(Scans.Read(_root, "a.txt"), Is.Not.Null);

    [Test]
    public void S2_MissingFileIsNull() =>
        Assert.That(Scans.Read(_root, "none.txt"), Is.Null);

    [Test]
    public void S3_Dots() => Assert.That(Attempt("../secret.txt"), Is.Null);

    [Test]
    public void S4_AbsoluteName() =>
        Assert.That(Attempt(Path.Combine(_top, "secret.txt")), Is.Null);

    [Test]
    public void S5_FolderNextDoor() =>
        Assert.That(Attempt("../library-secret/x.txt"), Is.Null);

    [Test]
    public void S6_HostileNameDoesNotThrow() =>
        Assert.DoesNotThrow(() => Scans.Read(_root, "../secret.txt"));
}
