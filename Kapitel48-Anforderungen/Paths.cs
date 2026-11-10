using NUnit.Framework;

namespace Library.Lending;

internal static class Paths
{
    // The folder with the project file, found from the test output.
    public static string Project
    {
        get
        {
            const string project = "Kapitel48-Anforderungen.csproj";
            DirectoryInfo? dir = new(TestContext.CurrentContext.TestDirectory);
            while (dir is not null
                && !File.Exists(Path.Combine(dir.FullName, project)))
            {
                dir = dir.Parent;
            }

            return dir?.FullName
                ?? throw new InvalidOperationException("no " + project);
        }
    }

    public static string Data(string name) =>
        Path.Combine(Project, "Data", name);
}
