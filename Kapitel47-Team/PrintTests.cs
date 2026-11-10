using NUnit.Framework;

namespace Library.Lending;

// Counts single-author files in a real repository. Run with:
// set REPO_DIR and REPO_PATH, then
// dotnet test --filter Category=Print --logger "console;verbosity=detailed"
[Category("Print")]
[Explicit("needs a real repository in REPO_DIR")]
public sealed class PrintTests
{
    [Test]
    public void PrintSingleAuthorFiles()
    {
        string repo = Environment.GetEnvironmentVariable("REPO_DIR") ?? ".";
        string path = Environment.GetEnvironmentVariable("REPO_PATH") ?? ".";
        foreach (bool co in new[] { false, true })
        {
            Dictionary<string, HashSet<string>> byFile =
                BusFactor.Authors(repo, co, path);
            int single = BusFactor.SingleAuthorFiles(byFile).Count;
            string people = string.Join(", ", byFile.Values
                .SelectMany(s => s).Distinct().Order(StringComparer.Ordinal));
            TestContext.Out.WriteLine(
                $"co-authors {co}: {single} of {byFile.Count} files " +
                $"have one author; authors: {people}");
        }
    }
}
