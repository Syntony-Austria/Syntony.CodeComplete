using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Library.Lending;

public sealed class RetentionTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 8, 6, 0, 0, TimeSpan.Zero);

    // A quiet service: one log file every sixth day, 31 files in all.
    private static string QuietService()
    {
        string folder = Directory.CreateTempSubdirectory("k44-").FullName;
        for (int day = 0; day <= 180; day += 6)
        {
            string file = Path.Combine(folder, $"lending-{day:000}.log");
            File.WriteAllText(file, "entry");
            File.SetLastWriteTimeUtc(file, Now.AddDays(-day).UtcDateTime);
        }

        return folder;
    }

    [Test]
    public void RotationKeepsHalfAYearOfAQuietService()
    {
        string folder = QuietService();

        int deleted = LogRetention.KeepNewest(folder, 31);

        DateTime oldest = new DirectoryInfo(folder).GetFiles("*.log")
            .Min(f => f.LastWriteTimeUtc);
        Assert.That(deleted, Is.Zero);
        Assert.That((Now.UtcDateTime - oldest).TotalDays, Is.EqualTo(180));
    }

    [Test]
    public void AnAgeLimitDeletesWhatIsOlderThanThirtyDays()
    {
        string folder = QuietService();

        int deleted = LogRetention.DeleteOlderThan(
            folder, TimeSpan.FromDays(30), new FakeTimeProvider(Now));

        Assert.That(deleted, Is.EqualTo(25));
        Assert.That(Directory.GetFiles(folder, "*.log"), Has.Length.EqualTo(6));
    }
}
