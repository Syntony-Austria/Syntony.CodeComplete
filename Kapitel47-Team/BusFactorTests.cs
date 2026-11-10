using NUnit.Framework;

namespace Library.Lending;

public sealed class BusFactorTests
{
    private static string s_repo = "";

    [OneTimeSetUp]
    public void BuildAMiniRepository()
    {
        s_repo = Path.Combine(Path.GetTempPath(), "k47-git-" + Guid.NewGuid());
        Directory.CreateDirectory(s_repo);
        BusFactor.Git(s_repo, "init", "-q");
        Commit("dev-1", "FeeSchedule.cs", "RenewalPolicy.cs", "DueDates.cs");
        Commit("dev-2", "ReturnDesk.cs", "RenewalPolicy.cs");
        Commit("dev-3", "Catalog.cs");
        Commit("dev-1", "FeeSchedule.cs", "Co-authored-by: dev-3 <dev-3@x.org>");
    }

    [Test]
    public void FourOfFiveFilesHaveOneAuthor()
    {
        Assert.That(BusFactor.SingleAuthorFiles(
            BusFactor.Authors(s_repo, withCoAuthors: false)), Is.EqualTo(new[]
        {
            "Catalog.cs (dev-3)", "DueDates.cs (dev-1)",
            "FeeSchedule.cs (dev-1)", "ReturnDesk.cs (dev-2)",
        }));
    }

    [Test]
    public void APairedCommitCountsTwoPeople()
    {
        Dictionary<string, HashSet<string>> byFile =
            BusFactor.Authors(s_repo, withCoAuthors: true);

        Assert.That(byFile["FeeSchedule.cs"], Is.EquivalentTo(new[]
        {
            "dev-1", "dev-3",
        }));
        Assert.That(BusFactor.SingleAuthorFiles(byFile), Has.Count.EqualTo(3));
    }

    // Each argument is a file to touch, or a trailer if it contains ": ".
    private static void Commit(string who, params string[] items)
    {
        foreach (string file in items.Where(i => !i.Contains(": ")))
        {
            File.AppendAllText(Path.Combine(s_repo, file), who + "\n");
        }

        string message = string.Join("\n\n",
            ["change by " + who, .. items.Where(i => i.Contains(": "))]);
        BusFactor.Git(s_repo, "add", ".");
        BusFactor.Git(s_repo, "-c", $"user.name={who}",
            "-c", $"user.email={who}@x.org", "commit", "-q", "-m", message);
    }
}
