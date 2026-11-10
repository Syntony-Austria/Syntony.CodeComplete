using NUnit.Framework;

namespace Library.Lending;

internal static class Paths
{
    // The folder with Kapitel47-Team.csproj, found from the test output.
    public static string Project
    {
        get
        {
            DirectoryInfo? dir = new(TestContext.CurrentContext.TestDirectory);
            const string project = "Kapitel47-Team.csproj";
            while (dir is not null
                && !File.Exists(Path.Combine(dir.FullName, project)))
            {
                dir = dir.Parent;
            }

            return dir?.FullName
                ?? throw new InvalidOperationException("no " + project);
        }
    }

    public static string Library => Path.Combine(Project, "Library");

    public static string Data(string name) => Path.Combine(Project, "Data", name);
}
