namespace Library.Lending;

public static class Paths
{
    // Finds the project folder from the test output folder.
    public static string Source(params string[] parts)
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.csproj").Length == 0)
        {
            dir = dir.Parent;
        }

        return Path.Combine([dir?.FullName
            ?? throw new DirectoryNotFoundException("project"), .. parts]);
    }
}
