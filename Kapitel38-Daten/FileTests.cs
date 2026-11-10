using System.Diagnostics;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class FileTests
{
    private const string Old = """{"version":1,"memberId":"m-1","feeInCents":50}""";
    private const string New = """{"version":1,"memberId":"m-1","feeInCents":75}""";

    private string _dir = "";

    [SetUp]
    public void CreateFolder()
    {
        _dir = Path.Combine(Path.GetTempPath(), $"loans-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void RemoveFolder() => Directory.Delete(_dir, recursive: true);

    // Writes the first half of the new content, then the power goes.
    private static void HalfThenCrash(Stream stream)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(New);
        stream.Write(bytes, 0, bytes.Length / 2);
        stream.Flush();
        throw new IOException("power cut");
    }

    private string FileWithOldContent()
    {
        string path = Path.Combine(_dir, "loan.json");
        File.WriteAllText(path, Old);
        return path;
    }

    [Test]
    public void ACrashWhileOverwritingLeavesHalfAFile()
    {
        string path = FileWithOldContent();

        Assert.Throws<IOException>(() =>
            SafeFile.WriteTangled(path, HalfThenCrash));

        string left = File.ReadAllText(path);
        TestContext.Out.WriteLine($"{left.Length} of {New.Length} chars: {left}");
        Assert.Catch<JsonException>(() => JsonDocument.Parse(left));
    }

    [Test]
    public void ACrashWhileWritingSafelyKeepsTheOldFile()
    {
        string path = FileWithOldContent();

        Assert.Throws<IOException>(() => SafeFile.Write(path, HalfThenCrash));

        Assert.Multiple(() =>
        {
            Assert.That(File.ReadAllText(path), Is.EqualTo(Old));
            Assert.That(Directory.GetFiles(_dir), Has.Length.EqualTo(1));
        });
    }

    [Test]
    public void ASafeWriteReplacesTheWholeFile()
    {
        string path = FileWithOldContent();

        SafeFile.Write(path, s => s.Write(Encoding.UTF8.GetBytes(New)));

        Assert.That(File.ReadAllText(path), Is.EqualTo(New));
    }

    // Not a rule, a measurement: what the flush to disk costs here.
    [Test]
    [Category("Measure")]
    public void FlushingToTheDiskCostsTime()
    {
        const int Files = 200;
        byte[] bytes = Encoding.UTF8.GetBytes(New);
        string path = Path.Combine(_dir, "loan.json");

        Stopwatch fast = Stopwatch.StartNew();
        for (int i = 0; i < Files; i++)
        {
            SafeFile.WriteWithoutFlush(path, s => s.Write(bytes));
        }

        fast.Stop();
        Stopwatch durable = Stopwatch.StartNew();
        for (int i = 0; i < Files; i++)
        {
            SafeFile.Write(path, s => s.Write(bytes));
        }

        durable.Stop();
        TestContext.Out.WriteLine(
            $"{Files} files: without flush {fast.ElapsedMilliseconds} ms, "
            + $"with flush {durable.ElapsedMilliseconds} ms");
        Assert.That(File.ReadAllText(path), Is.EqualTo(New));
    }
}
