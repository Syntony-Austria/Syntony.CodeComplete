using NUnit.Framework;

namespace Library.Lending;

public sealed class GitHistoryTests
{
    // Builds a repository with one commit "Close <id>" per finished piece,
    // dated on the day it was finished, and reads it back with git log.
    [Test]
    public void TheThroughputFromGitLogEqualsTheList()
    {
        List<WorkItem> items = DemoHistory.Create();
        string repo = Path.Combine(Path.GetTempPath(), "k49-" + Guid.NewGuid());
        Directory.CreateDirectory(repo);
        GitHistory.Git(repo, [], "init", "-q");
        foreach (WorkItem item in items.OrderBy(i => i.Finished))
        {
            string date = $"{item.Finished:yyyy-MM-dd}T12:00:00Z";
            GitHistory.Git(repo, new() { ["GIT_COMMITTER_DATE"] = date },
                "-c", "user.name=m-1", "-c", "user.email=m-1@example.invalid",
                "commit", "-q", "--allow-empty", "--date", date,
                "-m", "Close " + item.Id);
        }

        List<(string Id, DateOnly Done)> read = GitHistory.Read(repo);
        int[] fromGit = Flow.WeeklyThroughput(
            read.Select(r => new WorkItem(r.Id, r.Done, r.Done)),
            DemoHistory.FirstMonday, DemoHistory.Weeks);

        Assert.That(read, Has.Count.EqualTo(items.Count));
        Assert.That(fromGit, Is.EqualTo(Flow.WeeklyThroughput(
            items, DemoHistory.FirstMonday, DemoHistory.Weeks)));
    }
}
